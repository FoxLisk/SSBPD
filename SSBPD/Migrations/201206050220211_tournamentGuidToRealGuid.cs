namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class tournamentGuidToRealGuid : DbMigration
    {
        public override void Up()
        {
            AlterColumn("Tournaments", "TournamentGuid", c => c.Guid(nullable: false));
            AlterColumn("TournamentFiles", "TournamentGuid", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("TournamentFiles", "TournamentGuid", c => c.String(nullable: false, maxLength: 4000));
            AlterColumn("Tournaments", "TournamentGuid", c => c.String(nullable: false, maxLength: 4000));
        }
    }
}
