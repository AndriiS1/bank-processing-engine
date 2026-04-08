DROP TABLE IF EXISTS bank_payments.users;

CREATE TABLE bank_payments.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    amount BIGINT,
    status TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

DROP TABLE IF EXISTS bank_payments.outbox_messages;

CREATE TABLE bank_payments.outbox_messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    type TEXT NOT NULL,
    payload JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    processed_at TIMESTAMPTZ
);

DROP TABLE IF EXISTS bank_payments.processed_events;

CREATE TABLE bank_payments.processed_events (
    event_id UUID PRIMARY KEY DEFAULT gen_random_uuid()
);
