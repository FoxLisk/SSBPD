namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addImages1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Images", "MimeType", c => c.String(maxLength: 4000));
            AlterColumn("Images", "FileName", c => c.String(nullable: false, maxLength: 4000));
        }
        
        public override void Down()
        {
            AlterColumn("Images", "fileName", c => c.String(nullable: false, maxLength: 4000));
            DropColumn("Images", "MimeType");
        }
    }
}
