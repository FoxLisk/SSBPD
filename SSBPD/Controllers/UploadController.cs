using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Xml;
using SSBPD.Helper;
using System.Xml.Linq;
using SSBPD.Models;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SSBPD.Controllers
{
    public class UploadController : BaseController
    {
        private UserAuthHelper _userAuthHelper;
        public UserAuthHelper userAuthHelper
        {
            get
            {
                if (_userAuthHelper == null)
                {
                    _userAuthHelper = new UserAuthHelper();
                }
                return _userAuthHelper;
            }
            set
            {
                _userAuthHelper = value;
            }
        }
        private EraserHelper _eraserHelper;
        public EraserHelper eraserHelper
        {
            get
            {
                if (_eraserHelper == null)
                {
                    _eraserHelper = new EraserHelper();
                } return _eraserHelper;
            }
            set
            {
                _eraserHelper = value;
            }
        }

        public UploadController()
        {
            ViewBag.JavascriptIncludes = new List<string> { "~/Scripts/Upload.js" };
            ViewBag.CSSIncludes = new List<string> { "~/Content/Upload.css" };
        }
        public ActionResult Index()
        {
            if (Convert.ToInt32(Session["userId"]) > 0)
            {
                return View("LoggedInIndex");
            }
            return View("Index");
        }

        public ActionResult NewAccount()
        {

            string username = Request["username"];
            string password = Request["password"];
            string email = Request["emailAddress"];
            bool accountCreated = false;
            User newUser = null;
            try
            {
                newUser = userAuthHelper.createUser(username, password, email);
                accountCreated = true;
            }
            catch (InvalidEmailException e)
            {
                ViewBag.EmailError = "This is not a valid email.";
            }
            catch (UserExistsException e)
            {
                ViewBag.UsernameError = "This username is already in use.";
            }
            catch (EmailExistsException e)
            {
                ViewBag.EmailError = "This email is already in use.";
            }
            catch (PasswordTooWeakException e)
            {
                ViewBag.PasswordError = "This password is too weak. Please select a password of at least 8 characters.";
            }

            if (!accountCreated)
            {
                return View("Index");
            }
            Session["userId"] = newUser.UserID;
            return View("LoggedInIndex");
        }
        private delegate void asyncProcessFile(int tournamentFileId);

        private void processFileAndElo(int tournamentFileId)
        {
            TioParser tioparser = new TioParser(tournamentFileId);
            try
            {
                int tournamentId = tioparser.ParseTournament();
                ELOProcessor eloProcessor = new ELOProcessor();
                eloProcessor.adjustEloScoresForTournament(tournamentId);
            }
            catch (Exception)
            {
                return;
            }
        }

        [HttpPost]
        public ActionResult SeedTournament(HttpPostedFileBase file)
        {

            if (file == null || file.ContentLength == 0)
            {
                ViewBag.UploadMessage = "Please select a file for upload.";
                return View("Index");
            }
            string xml;
            using (StreamReader sr = new StreamReader(file.InputStream))
            {
                xml = sr.ReadToEnd();
            }
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch
            {
                ViewBag.UploadMessage = "Error: The TIO file you uploaded was not well-formed xml.";
                return View("Index");
            }
            TioParser tioParser = new TioParser(xml);
            XDocument returnXML;
            if (Request["bracketType"].Equals("pools"))
            {
                returnXML = tioParser.SeedTournamentForPools();
            }
            else
            {
                returnXML = tioParser.SeedTournamentForBracket();
            }
            byte[] xmlBytes = System.Text.Encoding.UTF8.GetBytes(returnXML.ToString());
            FileContentResult outFile = File(xmlBytes, "text/xml");
            outFile.FileDownloadName = file.FileName;
            return outFile;
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                ViewBag.UploadMessage = "Please select a file for upload.";
                return View("LoggedInIndex");
            }
            var userId = Convert.ToInt32(Session["userId"]);
            if (userId <= 0)
            {
                return View("Index");
            }

            string xml;
            using (StreamReader sr = new StreamReader(file.InputStream))
            {
                xml = sr.ReadToEnd();
            }

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch
            {
                ViewBag.UploadMessage = "Error: The TIO file you uploaded was not well-formed xml.";
                return View("LoggedInIndex");
            }

            Guid tioGuid = Guid.Parse(doc.SelectSingleNode("//EventList/Event/ID/text()").Value);
            
            TournamentFile foundFile = (from t in db.TournamentFiles
                                        where t.TournamentGuid.Equals(tioGuid)
                                        select t).FirstOrDefault();
            Tournament foundTournament = (from t in db.Tournaments
                                          where t.TournamentGuid.Equals(tioGuid)
                                          select t).FirstOrDefault();

            if (foundTournament != null || foundFile != null)
            {
                if (!Convert.ToBoolean(Session["userModerator"]))
                {
                    ViewBag.UploadMessage = "Error: This TIO file has been uploaded before.";
                    return View("LoggedInIndex");
                }
                else
                {
                    if (foundTournament != null)
                    {
                        eraserHelper.EraseTournament(foundTournament.TournamentID);
                    }
                    if (foundFile != null)
                    {
                        eraserHelper.EraseTournamentFile(foundFile.TournamentFileID);
                    }
                }
            }

            string originalFileName = new string(file.FileName.Take(32).ToArray());
            originalFileName = Regex.Replace(originalFileName, "\\s+", "");

            var fileName = originalFileName + DateTime.Now.ToString("yyyy-MM-dd-ffffff");

            string path = "/Files/UnauthorizedUploads";
            var filePath = Path.Combine(Server.MapPath(path), fileName);
            System.IO.File.WriteAllText(filePath, xml);

            TournamentFile tournamentFile = new TournamentFile();
            tournamentFile.XML = xml;
            tournamentFile.Processed = false;
            tournamentFile.Inserted = DateTime.Now;
            tournamentFile.OriginalFileName = originalFileName;
            tournamentFile.ProcessedAt = null;
            tournamentFile.TournamentGuid = tioGuid;
            tournamentFile.UserID = userId;
            db.TournamentFiles.Add(tournamentFile);
            db.SaveChanges();

            ViewBag.UploadMessage = "Thank you for your submission! It will be processed as soon as it's verified.";

            return View("LoggedInIndex");
        }

        [HttpPost]
        public ActionResult UploadImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                ViewBag.UploadMessage = "Please select a file for upload.";
                return View("Index");
            }
            byte[] imageBytes = new byte[file.ContentLength];
            file.InputStream.Read(imageBytes, 0, file.ContentLength);

            Image image = new Image();
            image.ImageBytes = imageBytes;
            image.FileName = file.FileName;
            image.MimeType = file.ContentType;
            db.Images.Add(image);
            db.SaveChanges();

            return View("Index");
        }
    }
}
