namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addModerator : DbMigration
    {
        public override void Up()
        {
            AddColumn("Users", "isModerator", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Users", "isModerator");
        }
    }
}
