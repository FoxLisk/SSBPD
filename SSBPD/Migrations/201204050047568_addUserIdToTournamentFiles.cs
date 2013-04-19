namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addUserIdToTournamentFiles : DbMigration
    {
        public override void Up()
        {
            AddColumn("TournamentFiles", "UserID", c => c.Int(nullable: false, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("TournamentFiles", "UserID");
        }
    }
}
