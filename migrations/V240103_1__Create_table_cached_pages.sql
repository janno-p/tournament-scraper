CREATE TABLE cached_pages (
    url TEXT PRIMARY KEY NOT NULL,
    tournament_id TEXT NOT NULL,
    html TEXT NOT NULL,
    updated INTEGER NOT NULL
);
