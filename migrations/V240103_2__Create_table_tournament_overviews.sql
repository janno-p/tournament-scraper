CREATE TABLE tournament_overviews (
    id UUID NOT NULL,
    last_changed TIMESTAMPTZ NOT NULL,
    CONSTRAINT pk_tournament_overviews PRIMARY KEY (id),
    CONSTRAINT pk_tournament_overviews_id FOREIGN KEY (id) REFERENCES tournaments (id)
);
