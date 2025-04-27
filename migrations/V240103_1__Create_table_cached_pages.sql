CREATE TABLE cached_pages (
    url VARCHAR(1000) NOT NULL,
    tournament_id UUID NOT NULL,
    html TEXT NOT NULL,
    updated TIMESTAMPTZ NOT NULL,
    CONSTRAINT pk_cached_pages PRIMARY KEY (url),
    CONSTRAINT fk_cached_pages_tournament_id FOREIGN KEY (tournament_id) REFERENCES tournaments (id)
);
