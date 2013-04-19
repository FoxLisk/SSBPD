namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addUserGuids : DbMigration
    {
        public override void Up()
        {
            AddColumn("Users", "UserGuid", c => c.Guid(nullable: false, defaultValueSql: "NEWID()"));
        }
        
        public override void Down()
        {
            DropColumn("Users", "UserGuid");
        }
    }
}
