using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SSBPD.Models;
using System.Data.Entity;
using SSBPD.Controllers;
using System.Data.Entity.Migrations;
using SSBPD.Helper;

namespace SSBPD
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private SSBPDContext db = new SSBPDContext();
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
        private static void RegisterSearchRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "advancedPlayerSearch",
                "search/player",
                new { controller = "Search", action = "PlayerSearch" }
            );
            routes.MapRoute(
                "advancedSetSearch",
                "search/set",
                new { controller = "Search", action = "SetSearch" }
            );
            routes.MapRoute(
                "searchIndex",
                "search",
                new { controller = "Search", action = "Index" }
            );
        }
        private static void RegisterSetRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "deleteSetLink",
                "deleteLink/{setLinkId}",
                new { controller = "SetLink", action = "DeleteSetLink" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );
            routes.MapRoute(
                "renameSetLink",
                "renameLink/{setLinkId}",
                new { controller = "SetLink", action = "RenameSetLink" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );
            routes.MapRoute(
                "linkLeaders",
                "linkLeaderBoard",
                new { controller = "SetLink", action = "LeaderBoard" }
            );
            routes.MapRoute(
                "setDetail",
                "set/{setId}",
                new { controller = "Set", action = "Detail" }
            );
            routes.MapRoute(
                "updateSet",
                "admin/updateSet/{setId}",
                new { controller = "Set", action = "UpdateSet" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );
            routes.MapRoute(
                "addVideo",
                "set/addVideo/{setId}",
                new { controller = "Set", action = "AddVideoLink" }
            );

        }
        private static void RegisterAdminRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "adminHome",
                "admin",
                new { controller = "Admin", action = "Index" }
            );
            routes.MapRoute(
                "logs",
                "admin/logs",
                new { controller = "Admin", action = "ViewLogs" }
            );
            routes.MapRoute(
                "adminLogin",
                "admin/login",
                new { controller = "Admin", action = "Login" }
            );
            routes.MapRoute(
                "releaseTournamentLock",
                "admin/releaseLock/{tournamentId}",
                new { controller = "Admin", action = "ReleaseLock" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );
            routes.MapRoute(
                "resetCharacterCache",
                "admin/resetcache/character",
                new { controller = "Admin", action = "ResetCharacterStatsCache" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );
            routes.MapRoute(
                "deletePlayer",
                "admin/deletePlayer/{playerId}",
                new { controller = "Admin", action = "DeletePlayer" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );
            routes.MapRoute(
                "assignSetsView",
                "admin/assignsets/{playerId}",
                new { controller = "Admin", action = "AssignSets" },
                new { httpMethod = new HttpMethodConstraint("GET") }
            );
            routes.MapRoute(
                "assignSets",
                "admin/assignsets/{playerId}",
                new { controller = "Admin", action = "DoAssignSets" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );
            routes.MapRoute(
                "getTournamentSelectForPlayer",
                "admin/tournamentSelect/{playerId}",
                new { controller = "Admin", action = "GetTournamentSelectForPlayer" }
            );
            routes.MapRoute(
                "createPlayerView",
                "admin/createplayer",
                new { controller = "Admin", action = "CreatePlayer" },
                new { httpMethod = new HttpMethodConstraint("GET") }
            );
            routes.MapRoute(
                "createPlayer",
                "admin/createplayer",
                new { controller = "Admin", action = "DoCreatePlayer" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );
            routes.MapRoute(
                 "mergePlayers",
                 "admin/mergePlayers",
                 new { controller = "Admin", action = "MergePlayers" },
                 new { httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapRoute(
                "updatePlayer",
                "admin/updatePlayer/{playerId}",
                new { controller = "Player", action = "UpdatePlayer" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );
            routes.MapRoute(
                "viewFlaggedPlayers",
                "admin/flaggedplayers",
                new { controller = "Admin", action = "FlaggedPlayers" }
            );
            routes.MapRoute(
                "viewFlaggedSets",
                "admin/flaggedSets",
                new { controller = "Admin", action = "FlaggedSets" }
            );
            routes.MapRoute(
                "uploadImagePage",
                "admin/uploadImage",
                new { controller = "Admin", action = "UploadImages" }
            );

            routes.MapRoute(
                "mergePlayersPost",
                "admin/mergePlayers",
                new { controller = "Admin", action = "DoMergePlayers" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );

            routes.MapRoute(
                "viewTournamentXML",
                "admin/tournamentxml/{tournamentFileID}",
                new { controller = "Admin", action = "ViewTournamentXML" }
            );

            routes.MapRoute(
                "processTournamentFile",
                "admin/processFile/{tournamentFileId}",
                new { controller = "Admin", action = "ProcessTournamentFile", tournamentFileId = UrlParameter.Optional }
            );
            routes.MapRoute(
                "processEloForTournament",
                "admin/processElo/{tournamentId}",
                new { controller = "Admin", action = "ProcessEloForTournament" }
            );
            routes.MapRoute(
                "eraseTournament",
                "admin/eraseTournament/{tournamentId}",
                new { controller = "Admin", action = "EraseTournament" }
            );
            routes.MapRoute(
                "erasePlayerFlags",
                "admin/erasePlayerFlags/{playerId}",
                new { controller = "Admin", action = "ErasePlayerFlags" }
            );
            routes.MapRoute(
                "erasePlayerRegionFlags",
                "admin/erasePlayerRegionFlags/{playerId}",
                new { controller = "Admin", action = "ErasePlayerRegionFlags" }
            );
            routes.MapRoute(
                "erasePlayerCharacterFlags",
                "admin/erasePlayerCharacterFlags/{playerId}",
                new { controller = "Admin", action = "ErasePlayerCharacterFlags" }
            );
            routes.MapRoute(
                "eraseTournamentFile",
                "admin/eraseTournamentFile/{tournamentFileId}",
                new { controller = "Admin", action = "EraseTournamentFile" }
            );

            routes.MapRoute(
                "resetEloScores",
                "admin/resetEloScores",
                new { controller = "Admin", action = "ResetAllEloScores" }
            );
            routes.MapRoute(
                "processAllTournaments",
                "admin/processAllTournaments",
                new { controller = "Admin", action = "ProcessAllTournaments" }
            );

            routes.MapRoute(
                "makeUserAdmin",
                "admin/changeAdminStatus/{userID}",
                new { controller = "Admin", action = "ChangeAdminStatus" }
            );

            routes.MapRoute(
                "changeModStatus",
                "admin/changeModStatus/{userID}",
                new { controller = "Admin", action = "ChangeModeratorStatus" }
            );
        }
        private static void RegisterPlayerRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "player",
                "player",
                new { controller = "Player", action = "Index" }
            );
            routes.MapRoute(
                "playerSearch",
                "player/search",
                new { controller = "Player", action = "Search" }
            );
            routes.MapRoute(
                "playerVersus",
                "player/{playerOneTag}/versus/{playerTwoTag}",
                new { controller = "Player", action = "Versus" }
            );
            routes.MapRoute(
                "playerDetail",
                "player/{tag}",
                new { controller = "Player", action = "Detail" }
            );

        }
        private static void RegisterTournamentRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "tournament",
                "tournament/",
                new { controller = "Tournament", action = "Index" }
            );
            routes.MapRoute(
                "tournamentSearch",
                "tournament/search",
                new { controller = "Tournament", action = "Search" }
            );
            routes.MapRoute(
                "viewBracket",
                "tournament/bracket/{tournamentId}/{bracketName}",
                new { controller = "Tournament", action = "ViewBracket" }
            );
            routes.MapRoute(
                "tournamentDetail",
                "tournament/{tournamentName}",
                new { controller = "Tournament", action = "Detail", tournamentName = UrlParameter.Optional }
            );
            routes.MapRoute(
                "tournamentUpdate",
                "tournament/update/{tournamentId}",
                new { controller = "Tournament", action = "Update" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );
        }
        private static void RegisterRegionRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "regionGroups",
                "region",
                new { controller = "Region", action = "RegionGroup" }
            );
            routes.MapRoute(
                "regionIndex",
                "region/index",
                new { controller = "Region", action = "Index" }
            );
            routes.MapRoute(
                "regionVersus",
                "region/versus",
                new { controller = "Region", action = "Versus" }
            );

            routes.MapRoute(
                "regionVersusSets",
                "region/versus/sets",
                new { controller = "Region", action = "VersusSets" }
            );
            routes.MapRoute(
                "region",
                "region/{regionValue}",
                new { controller = "Region", action = "Region" }
            );
        }
        private static void RegisterUploadRoutes(RouteCollection routes)
        {

            routes.MapRoute(
                "uploadIndex",
                "upload",
                new { controller = "Upload", action = "Index" },
                new { httpMethod = new HttpMethodConstraint("GET") }
            );
            routes.MapRoute(
                "uploadPost",
                "upload",
                new { controller = "Upload", action = "Upload" },
                new { httpMethod = new HttpMethodConstraint("Post") }
            );
            routes.MapRoute(
                "uploadForSeeding",
                "upload/seeding",
                new { controller = "Upload", action = "SeedTournament" },
                new { httpMethod = new HttpMethodConstraint("Post") }
            );
            routes.MapRoute(
                "uploadImage",
                "uploadImage",
                new { controller = "Upload", action = "UploadImage" },
                new { httpMethod = new HttpMethodConstraint("Post") }
            );
            routes.MapRoute(
                "uploadNewAccount",
                "upload/newaccount",
                new { controller = "Upload", action = "NewAccount" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );
        }
        private static void RegisterLoginRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "login",
                "login",
                new { controller = "Login", action = "LogIn" }
            );

            routes.MapRoute(
                "logout",
                "logout",
                new { controller = "Login", action = "LogOut" }
            );

            routes.MapRoute(
                "createAccountDialog",
                "createaccount",
                new { controller = "Login", action = "CreateAccountDialog" },
                new { httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapRoute(
                "createAccount",
                "createaccount",
                new { controller = "Login", action = "CreateAccount" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );


        }
        private static void RegisterCharacterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "character",
                "character",
                new { controller = "Character", action = "Index" }
            );
            routes.MapRoute(
                "characterStats",
                "character/stats",
                new { controller = "Character", action = "Stats" }
            );
            routes.MapRoute(
                "characterDetail",
                "character/{characterName}",
                new { controller = "Character", action = "Detail" }
            );
        }
        private static void RegisterFlagRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "addPlayerFlag",
                "flagPlayer",
                new { controller = "Flag", action = "AddPlayerFlag" },
                new { httpMethod = new HttpMethodConstraint("Post") }
            );
            routes.MapRoute(
                "addRegionFlag",
                "flagRegion/{playerId}",
                new { controller = "Flag", action = "AddRegionFlag" },
                new { httpMethod = new HttpMethodConstraint("Post") }
            );
            routes.MapRoute(
                "addCharacterFlagForPlayer",
                "flagCharacter/player/{playerId}",
                new { controller = "Flag", action = "addCharacterFlagForPlayer" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );
            routes.MapRoute(
                "addCharacterFlagForSet",
                "flagCharacter/set/{setId}",
                new { controller = "Flag", action = "addCharacterFlagForSet" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );
            routes.MapRoute(
                "addSetLinkFlag",
                "flagLink/{setLinkId}",
                new { controller = "Flag", action = "AddSetLinkFlag" },
                new { httpMethod = new HttpMethodConstraint("POST") }
             );


        }
        private static void RegisterDownloadRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "excel",
                "download/excel",
                new { controller = "Download", action = "Excel" }
            );
            routes.MapRoute(
                "downloadTio",
                "download/{tournamentGuid}",
                new { controller = "Download", action = "TioFile" }
            );
            routes.MapRoute(
                "image",
                "images/{fileName}",
                new { controller = "Download", action = "Image" }
            );


        }
        private static void RegisterUserRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                   "userHome",
                   "user",
                   new { controller = "User", action = "Index" }
               );
            routes.MapRoute(
                "CustomRegions",
                "user/customRegions",
                new { controller = "User", action = "CustomRegions" }
            );
            routes.MapRoute(
                "userUpdate",
                "user/update",
                new { controller = "User", action = "Update" }
            );
            routes.MapRoute(
                "userReset",
                "user/resetPassword",
                new { controller = "User", action = "ResetPassword" }
            );
            routes.MapRoute(
                "sendResetPassword",
                "user/sendReset/{email}",
                new { controller = "User", action = "SendResetEmail" }
            );

        }
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            RegisterAdminRoutes(routes);
            RegisterPlayerRoutes(routes);
            RegisterLoginRoutes(routes);
            RegisterTournamentRoutes(routes);
            
            RegisterRegionRoutes(routes);
            RegisterUploadRoutes(routes);
            RegisterCharacterRoutes(routes);
            RegisterFlagRoutes(routes);
            RegisterDownloadRoutes(routes);
            RegisterSetRoutes(routes);
            RegisterUserRoutes(routes);
            RegisterSearchRoutes(routes);

            routes.MapRoute(
                "createRegion",
                "customregions/create",
                new { controller = "CustomRegion", action = "Create" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );
            routes.MapRoute(
                "deleteRegion",
                "customregions/delete/{customRegionId}",
                new { controller = "CustomRegion", action = "Delete" },
                new { httpMethod = new HttpMethodConstraint("POST") }
            );
            
            routes.MapRoute(
                "home",
                "",
                new { controller = "Home", action = "Index" }
            );
            routes.MapRoute(
                "homeIndex",
                "index",
                new { controller = "Home", action = "Index" }
            );
            routes.MapRoute(
                "about",
                "about",
                new { controller = "Home", action = "About" }
            );
            routes.MapRoute(
                "Exception",
                "exception",
                new { controller = "Error", action = "Exception" }
            );
            /*
            routes.MapRoute(
                "Any",
                "{*any}",
                new { controller = "Home", action = "Index" }
            );
             */
            routes.MapRoute(
                "Error", // Route name
                "{*path}", // URL with parameters
                new { controller = "Error", action = "Error", id = UrlParameter.Optional } // Parameter defaults
            );


        }
        protected void Session_OnStart()
        {
            HttpCookie userCookie = Request.Cookies["userID"];

            if (userCookie == null) return;
            Guid userGuid;
            bool isGuid = Guid.TryParse(userCookie.Value, out userGuid);
            if (!isGuid) return;
            User user = db.Users.Where(u => u.UserGuid == userGuid).FirstOrDefault();
            if (user == null) return;

            Session["userId"] = user.UserID;
            if (user.isAdmin)
            {
                Session["userAdmin"] = true;
            }
            if (user.isModerator)
            {
                Session["userModerator"] = true;
            }

        }

        protected void Application_Start()
        {
            HttpContext.Current.Application["startup"] = DateTime.Now;
//#if DEBUG
            DbMigrator dbMigrator = new DbMigrator(new SSBPD.Migrations.Configuration());
            dbMigrator.Update();
//#endif
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

    }
}