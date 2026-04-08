DROP TABLE bank_payments.users;

CREATE TABLE bank_payments.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    amount BIGINT,
    status TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

DROP TABLE bank_payments.outbox_messages;

CREATE TABLE bank_payments.outbox_messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    type TEXT NOT NULL,
    payload JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    processed_at TIMESTAMPTZ
);
