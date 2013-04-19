namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddCharacters : DbMigration
    {
        public override void Up()
        {
            AddColumn("Sets", "WinnerCharacterID", c => c.Int());
            AddColumn("Sets", "LoserCharacterID", c => c.Int());
            AddColumn("Players", "CharacterMainID", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("Players", "CharacterMainID");
            DropColumn("Sets", "LoserCharacterID");
            DropColumn("Sets", "WinnerCharacterID");
        }
    }
}
