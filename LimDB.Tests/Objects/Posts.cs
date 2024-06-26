﻿using LimDB.lib.Objects.Base;

namespace LimDB.Tests.Objects
{
    public class Posts : BaseObject
    {
        public required string Title { get; set; }

        public required string Body { get; set; }

        public required string Category { get; set; }

        public required string URL { get; set; }

        public required DateTime PostDate { get; set; }
    }
}