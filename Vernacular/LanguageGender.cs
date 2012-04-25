namespace Vernacular
{
    public enum LanguageGender
    {
        Neutral,
        Masculine,
        Feminine
    }

    public interface ILanguageGenderProvider
    {
        LanguageGender LanguageGender { get; }
    }
}