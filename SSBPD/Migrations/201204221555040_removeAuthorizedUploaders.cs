namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class removeAuthorizedUploaders : DbMigration
    {
        public override void Up()
        {
            DropColumn("Users", "authorizedUploader");
            DropColumn("TournamentFiles", "AuthorizedForProcessing");
        }
        
        public override void Down()
        {
            AddColumn("TournamentFiles", "AuthorizedForProcessing", c => c.Boolean(nullable: false));
            AddColumn("Users", "authorizedUploader", c => c.Boolean(nullable: false));
        }
    }
}
