namespace GitLab.VisualStudio.Shared.Models
{
    public class NamespacesPath
    {
        public static implicit operator NamespacesPath(NGitLab.Models.Namespaces p)
        {
            if (p != null)
            {
                return new NamespacesPath()
                {
                    id = p.Id,
                    kind = p.Kind,
                    name = p.Name,
                    path = p.Path,
                    full_path = p.FullPath
                };
            }
            else
            {
                return null;
            }
        }

        public int id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public string kind { get; set; }
        public string full_path { get; set; }
    }
}