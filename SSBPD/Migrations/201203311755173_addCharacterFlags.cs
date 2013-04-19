namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addCharacterFlags : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "CharacterFlags",
                c => new
                    {
                        CharacterFlagID = c.Int(nullable: false, identity: true),
                        PlayerID = c.Int(),
                        SetID = c.Int(),
                        WinnerFlag = c.Boolean(),
                        UserID = c.Int(nullable: false),
                        CharacterID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CharacterFlagID);
            
        }
        
        public override void Down()
        {
            DropTable("CharacterFlags");
        }
    }
}
