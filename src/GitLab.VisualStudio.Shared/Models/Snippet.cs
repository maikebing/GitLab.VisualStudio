namespace GitLab.VisualStudio.Shared.Models
{
    public class Snippet
    {
        public static implicit operator Snippet(NGitLab.Models.ProjectSnippet p)
        {
            if (p != null)
            {
                return new Snippet()
                {
                    Description = p.Description,
                    FileName = p.FileName,
                    Id = p.Id,
                    Title = p.Title,
                    WebUrl = p.WebUrl
                };
            }
            else
            {
                return null;
            }
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public string WebUrl { get; set; }
    }
}