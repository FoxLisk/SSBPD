namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addRegions : DbMigration
    {
        public override void Up()
        {
            AddColumn("Players", "RegionValue", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("Players", "RegionValue");
        }
    }
}
