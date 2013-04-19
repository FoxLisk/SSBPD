namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class removeBestOf : DbMigration
    {
        public override void Up()
        {
            DropColumn("Sets", "BestOf");
        }
        
        public override void Down()
        {
            AddColumn("Sets", "BestOf", c => c.Int());
        }
    }
}
