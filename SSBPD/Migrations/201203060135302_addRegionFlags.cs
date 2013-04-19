namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class addRegionFlags : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "RegionFlags",
                c => new
                {
                    RegionFlagID = c.Int(nullable: false, identity: true),
                    PlayerID = c.Int(nullable: false),
                    userID = c.Int(nullable: false),
                    RegionValue = c.Int(nullable: false)
                })
                .PrimaryKey(t => t.RegionFlagID);

        }

        public override void Down()
        {
            DropTable("RegionFlags");
        }
    }
}
