CREATE TYPE event_type AS ENUM ('MD', 'MS', 'WD', 'WS', 'XD');

CREATE TYPE event_league AS ENUM ('MASTERS', 'FIRST', 'SECOND', 'THIRD', 'FOURTH');

CREATE TABLE tournament_events (
    id UUID NOT NULL,
    tournament_id UUID NOT NULL,
    url VARCHAR(1000) NOT NULL,
    name VARCHAR(150) NOT NULL,
    event_type event_type,
    event_type_guess event_type,
    event_league event_league,
    event_league_guess event_league,
    CONSTRAINT pk_tournament_events PRIMARY KEY (id),
    CONSTRAINT fk_tournament_events_tournament_id FOREIGN KEY (tournament_id) REFERENCES tournaments (id),
    CONSTRAINT uq_tournament_events_url UNIQUE (url)
);
