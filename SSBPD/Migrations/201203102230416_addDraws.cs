namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addDraws : DbMigration
    {
        public override void Up()
        {
            AddColumn("Sets", "isDraw", c => c.Boolean(nullable: false));
            AddColumn("Sets", "Wins", c => c.Int());
            Sql("UPDATE Sets SET isDraw = 0");
            Sql("UPDATE Sets SET Wins = ( (bestOf + 1 ) / 2 ) where isPool = 1");
        }
        
        public override void Down()
        {
            DropColumn("Sets", "Wins");
            DropColumn("Sets", "isDraw");
        }
    }
}
