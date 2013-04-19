namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class removeFirstAndLastName : DbMigration
    {
        public override void Up()
        {
            DropColumn("Players", "firstName");
            DropColumn("Players", "lastName");
        }
        
        public override void Down()
        {
            AddColumn("Players", "lastName", c => c.String(maxLength: 4000));
            AddColumn("Players", "firstName", c => c.String(maxLength: 4000));
        }
    }
}
