CREATE TABLE tournaments (
    id TEXT NOT NULL PRIMARY KEY,
    url TEXT NOT NULL,
    name TEXT NOT NULL,
    start_date BIGINT NOT NULL,
    end_date BIGINT NOT NULL
);
