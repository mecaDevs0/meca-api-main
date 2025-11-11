-- Script para configurar banco de dados RDS com todas as tabelas necessárias

-- 1. Tabela de histórico de manutenção
CREATE TABLE IF NOT EXISTS maintenance_history (
    id VARCHAR(255) PRIMARY KEY,
    vehicle_id VARCHAR(255) NOT NULL,
    customer_id VARCHAR(255) NOT NULL,
    workshop_id VARCHAR(255) NOT NULL,
    workshop_name VARCHAR(255) NOT NULL,
    service_id VARCHAR(255) NOT NULL,
    service_name VARCHAR(255) NOT NULL,
    service_description TEXT,
    completion_date TIMESTAMP NOT NULL,
    price_paid DECIMAL(10,2) NOT NULL,
    notes TEXT,
    workshop_notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. Adicionar campos de evidência na tabela bookings
ALTER TABLE bookings 
ADD COLUMN IF NOT EXISTS evidence_urls TEXT[],
ADD COLUMN IF NOT EXISTS payment_id VARCHAR(255),
ADD COLUMN IF NOT EXISTS payment_status VARCHAR(50),
ADD COLUMN IF NOT EXISTS paid_at TIMESTAMP,
ADD COLUMN IF NOT EXISTS payment_data JSONB;

-- 3. Adicionar campo de parcelamento na tabela workshops
ALTER TABLE workshops 
ADD COLUMN IF NOT EXISTS accepts_installment BOOLEAN DEFAULT FALSE;

-- 4. Atualizar todas as oficinas existentes para aceitar parcelamento
UPDATE workshops 
SET accepts_installment = TRUE 
WHERE accepts_installment IS NULL OR accepts_installment = FALSE;

-- 5. Criar índices para performance
CREATE INDEX IF NOT EXISTS idx_maintenance_history_vehicle_id ON maintenance_history(vehicle_id);
CREATE INDEX IF NOT EXISTS idx_maintenance_history_customer_id ON maintenance_history(customer_id);
CREATE INDEX IF NOT EXISTS idx_maintenance_history_workshop_id ON maintenance_history(workshop_id);
CREATE INDEX IF NOT EXISTS idx_maintenance_history_completion_date ON maintenance_history(completion_date);

CREATE INDEX IF NOT EXISTS idx_bookings_payment_id ON bookings(payment_id);
CREATE INDEX IF NOT EXISTS idx_bookings_payment_status ON bookings(payment_status);

-- 6. Criar tabela de notificações
CREATE TABLE IF NOT EXISTS notifications (
    id VARCHAR(255) PRIMARY KEY,
    user_id VARCHAR(255) NOT NULL,
    user_type VARCHAR(50) NOT NULL, -- 'customer' ou 'workshop'
    type VARCHAR(100) NOT NULL,
    title VARCHAR(255) NOT NULL,
    message TEXT NOT NULL,
    data JSONB,
    read_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_notifications_user_id ON notifications(user_id);
CREATE INDEX IF NOT EXISTS idx_notifications_user_type ON notifications(user_type);
CREATE INDEX IF NOT EXISTS idx_notifications_created_at ON notifications(created_at);

-- 7. Criar tabela de configurações de parcelamento
CREATE TABLE IF NOT EXISTS installment_settings (
    id VARCHAR(255) PRIMARY KEY,
    workshop_id VARCHAR(255) NOT NULL,
    max_installments INTEGER DEFAULT 12,
    min_installment_value DECIMAL(10,2) DEFAULT 50.00,
    interest_rate DECIMAL(5,2) DEFAULT 0.00,
    active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_installment_settings_workshop_id ON installment_settings(workshop_id);

-- 8. Criar tabela de cartões salvos (tokenizados)
CREATE TABLE IF NOT EXISTS saved_cards (
    id VARCHAR(255) PRIMARY KEY,
    customer_id VARCHAR(255) NOT NULL,
    card_token VARCHAR(255) NOT NULL UNIQUE,
    card_brand VARCHAR(50),
    last_four_digits VARCHAR(4),
    cardholder_name VARCHAR(255),
    is_default BOOLEAN DEFAULT FALSE,
    active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_saved_cards_customer_id ON saved_cards(customer_id);
CREATE INDEX IF NOT EXISTS idx_saved_cards_card_token ON saved_cards(card_token);

-- 9. Criar tabela de logs de webhook
CREATE TABLE IF NOT EXISTS webhook_logs (
    id VARCHAR(255) PRIMARY KEY,
    source VARCHAR(100) NOT NULL, -- 'pagbank', 'stripe', etc.
    event_type VARCHAR(100) NOT NULL,
    payload JSONB NOT NULL,
    processed BOOLEAN DEFAULT FALSE,
    error_message TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_webhook_logs_source ON webhook_logs(source);
CREATE INDEX IF NOT EXISTS idx_webhook_logs_event_type ON webhook_logs(event_type);
CREATE INDEX IF NOT EXISTS idx_webhook_logs_processed ON webhook_logs(processed);

-- 10. Inserir configurações padrão de parcelamento para todas as oficinas
INSERT INTO installment_settings (id, workshop_id, max_installments, min_installment_value, interest_rate, active)
SELECT 
    'installment_' || id,
    id,
    12,
    50.00,
    0.00,
    TRUE
FROM workshops 
WHERE id NOT IN (SELECT workshop_id FROM installment_settings);

-- 11. Criar função para atualizar timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- 12. Criar triggers para atualizar updated_at
CREATE TRIGGER update_maintenance_history_updated_at 
    BEFORE UPDATE ON maintenance_history 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_installment_settings_updated_at 
    BEFORE UPDATE ON installment_settings 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_saved_cards_updated_at 
    BEFORE UPDATE ON saved_cards 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- 13. Criar view para estatísticas de manutenção
CREATE OR REPLACE VIEW maintenance_stats AS
SELECT 
    v.id as vehicle_id,
    v.plate,
    v.brand,
    v.model,
    v.year,
    COUNT(mh.id) as total_services,
    SUM(mh.price_paid) as total_spent,
    MAX(mh.completion_date) as last_service_date,
    AVG(mh.price_paid) as avg_service_price
FROM vehicles v
LEFT JOIN maintenance_history mh ON v.id = mh.vehicle_id
GROUP BY v.id, v.plate, v.brand, v.model, v.year;

-- 14. Criar view para estatísticas de oficinas
CREATE OR REPLACE VIEW workshop_stats AS
SELECT 
    w.id as workshop_id,
    w.name as workshop_name,
    COUNT(b.id) as total_bookings,
    COUNT(CASE WHEN b.status = 'completed' THEN 1 END) as completed_bookings,
    COUNT(CASE WHEN b.status = 'paid' THEN 1 END) as paid_bookings,
    SUM(CASE WHEN b.status = 'paid' THEN b.total_amount ELSE 0 END) as total_revenue,
    AVG(CASE WHEN b.status = 'paid' THEN b.total_amount END) as avg_booking_value
FROM workshops w
LEFT JOIN bookings b ON w.id = b.workshop_id
GROUP BY w.id, w.name;

COMMIT;








