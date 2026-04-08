INSERT INTO bank_payments.users (amount, status)
VALUES (100000, 'Ready'),
       (983000, 'Deleted');

INSERT INTO bank_payments.outbox_messages (type, payload)
VALUES ('Created', '{}'),
       ('Processed', '{}')