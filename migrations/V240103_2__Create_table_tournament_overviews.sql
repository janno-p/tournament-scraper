CREATE TABLE tournament_overviews (
    id TEXT PRIMARY KEY NOT NULL,
    last_changed INTEGER NOT NULL,
    FOREIGN KEY(id) REFERENCES tournaments(id)
);
