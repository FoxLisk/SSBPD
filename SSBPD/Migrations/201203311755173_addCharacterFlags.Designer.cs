// <auto-generated />
namespace SSBPD.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    
    public sealed partial class addCharacterFlags : IMigrationMetadata
    {
        string IMigrationMetadata.Id
        {
            get { return "201203311755173_addCharacterFlags"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return "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"; }
        }
    }
}