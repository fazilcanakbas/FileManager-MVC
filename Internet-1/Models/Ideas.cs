using System;
using System.Collections.Generic;

namespace Internet_1.Models
{
    public class Ideas
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } // Tüm kodlarda Description olarak kullanılmalı
        public string Category { get; set; }
        public int Likes { get; set; }
       
        //public string UserName { get; set; }

        // Yorumlar (Feedbacks)
        public ICollection<IdeasFeedback> Feedbacks { get; set; } = new List<IdeasFeedback>();
    }

    public class IdeasFeedback
    {
        public int Id { get; set; }
        public int IdeaId { get; set; }
        public string Comment { get; set; }
        public string UserName { get; set; }
        public bool IsLiked { get; set; }

        // İlişkilendirme
        public Ideas Idea { get; set; }
    }
}
