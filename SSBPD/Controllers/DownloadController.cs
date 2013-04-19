using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;
using System.Net.Mime;
using System.Xml.Linq;
using System.Globalization;
using SSBPD.Helper;
using System.IO;
using ExcelLibrary.SpreadSheet;
using System.Xml;

namespace SSBPD.Controllers
{
    public class DownloadController : BaseController
    {
        CultureInfo[] knownFuckedCultures = new CultureInfo[1] {new CultureInfo("en-AU")};

        public ActionResult TioFile(string tournamentGuid)
        {
            CultureInfo toCi = new CultureInfo("en-us");
            if (Request["cultureCode"] != null)
            {
                try
                {
                    toCi = new CultureInfo(Request["cultureCode"]);
                }
                catch (CultureNotFoundException e)
                {
                }
            }
            var tournamentFile = (from tf in db.TournamentFiles
                                  where tf.TournamentGuid.Equals(new Guid(tournamentGuid))
                                  select tf).FirstOrDefault();
            if (tournamentFile == null)
            {
                return null;
            }

            XDocument safeXML = TioParser.getReadableXML(tournamentFile.XML);
            string returnXML = setDatesForTimeZoneInXML(safeXML, toCi);
            byte[] xmlBytes = System.Text.Encoding.UTF8.GetBytes(returnXML);
            FileContentResult outFile = File(xmlBytes, "text/xml");
            outFile.FileDownloadName = tournamentFile.OriginalFileName;
            return outFile;
        }

        public ActionResult Image(string fileName)
        {
            SSBPD.Models.Image image = (from i in db.Images
                                        where i.FileName.Equals(fileName)
                                        select i).FirstOrDefault();
            if (image == null)
            {
                return null;
            }
            return File(image.ImageBytes, image.MimeType ?? "image/png");
        }

        public ActionResult Excel()
        {
            Workbook eloOut = new Workbook();
            Worksheet sheet1 = new Worksheet("Scores");

            var players = from p in db.Players
                          select p;
            var tournaments = from t in db.Tournaments
                              orderby t.Date ascending
                              select t;
            int col = 1;
            var tournamentMap = new Dictionary<int, int>(); //tournamentId => column
            foreach (var tournament in tournaments)
            {
                sheet1.Cells[0, col] = new Cell(tournament.Name);
                tournamentMap[tournament.TournamentID] = col;
                col++;
            }

            int row = 1;
            foreach (var player in players)
            {
                sheet1.Cells[row, 0] = new Cell(player.Tag);
                var scores = from e in db.EloScores
                             where e.PlayerID == player.PlayerId
                             select e;
                foreach (var score in scores)
                {
                    if (tournamentMap.ContainsKey(score.TournamentID))
                    {
                        sheet1.Cells[row, tournamentMap[score.TournamentID]] = new Cell(score.ELO);
                    }
                }
                row++;
            }
            eloOut.Worksheets.Add(sheet1);
            var byteStream = new MemoryStream();
            eloOut.Save(byteStream);
            byteStream.Seek(0, SeekOrigin.Begin);
            var outf = File(byteStream, "application/vnd.ms-excel", "scores.xls");
            return outf;
        }


        private string setDatesForTimeZoneInXML(XDocument tournamentXML, CultureInfo toCi)
        {
            CultureInfo fromCi = new CultureInfo(tournamentXML.Descendants("Culture").First().Value, false);
            tournamentXML.Descendants("Culture").First().Value = toCi.IetfLanguageTag;
            string[] elementsToChange = new string[] { "StartDate", "BracketStart", "BracketEnd", "OriginalEstimate", "Date" };
            foreach (string elementName in elementsToChange)
            {
                foreach (XElement element in tournamentXML.Descendants(elementName))
                {
                    try
                    {
                        DateTime fromDate = DateTime.Parse(element.Value, fromCi);
                        element.Value = fromDate.ToString(toCi);
                    }
                    catch // apparently you can get a tio file to say the culture is en-US and have your date in d/m/y format.
                    {
                        foreach (var ci in knownFuckedCultures)
                        {
                            bool isValid = false;
                            DateTime fromDate = DateTime.Now;
                            isValid = DateTime.TryParse(element.Value, ci, DateTimeStyles.None, out fromDate);
                            if (isValid)
                            {
                                element.Value = fromDate.ToString(toCi);
                                break;
                            }
                        }
                    }
                }
            }
            return tournamentXML.ToString();
        }

    }
}
