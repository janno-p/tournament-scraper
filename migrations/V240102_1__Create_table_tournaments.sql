CREATE TABLE tournaments (
    id UUID NOT NULL,
    url VARCHAR(1000) NOT NULL,
    name VARCHAR(150) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    CONSTRAINT pk_tournaments PRIMARY KEY (id),
    CONSTRAINT uq_tournaments_url UNIQUE (url)
);
