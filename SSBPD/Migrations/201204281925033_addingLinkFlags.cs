namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addingLinkFlags : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "SetLinkFlags",
                c => new
                    {
                        SetLinkFlagID = c.Int(nullable: false, identity: true),
                        SetLinkID = c.Int(nullable: false),
                        userID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SetLinkFlagID);
            
        }
        
        public override void Down()
        {
            DropTable("SetLinkFlags");
        }
    }
}
