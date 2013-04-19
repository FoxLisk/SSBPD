namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addImages : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Images",
                c => new
                    {
                        ImageID = c.Int(nullable: false, identity: true),
                        ImageBytes = c.Binary(nullable: false),
                        fileName = c.String(nullable: false, maxLength: 4000),
                    })
                .PrimaryKey(t => t.ImageID);
            
        }
        
        public override void Down()
        {
            DropTable("Images");
        }
    }
}
