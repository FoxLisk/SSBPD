namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addCustomRegions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "CustomRegions",
                c => new
                    {
                        CustomRegionID = c.Int(nullable: false, identity: true),
                        Value = c.String(nullable: false, maxLength: 4000),
                        Name = c.String(nullable: false, maxLength: 4000),
                        UserID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CustomRegionID);
            
        }
        
        public override void Down()
        {
            DropTable("CustomRegions");
        }
    }
}
