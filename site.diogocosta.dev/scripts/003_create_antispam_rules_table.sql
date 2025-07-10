-- Script para criar tabela de regras anti-spam dinâmicas
-- Data: 2025-07-10
-- Descrição: Permite adicionar/remover regras de bloqueio em tempo real

-- Criar tabela de regras anti-spam
CREATE TABLE IF NOT EXISTS public.antispam_rules (
    id SERIAL PRIMARY KEY,
    rule_type VARCHAR(50) NOT NULL, -- 'ip', 'domain', 'email_pattern', 'name_pattern', 'user_agent'
    rule_value TEXT NOT NULL, -- O valor da regra (IP, domínio, padrão, etc)
    description TEXT, -- Descrição da regra
    severity VARCHAR(20) DEFAULT 'medium', -- 'low', 'medium', 'high', 'critical'
    is_active BOOLEAN DEFAULT true,
    is_regex BOOLEAN DEFAULT false, -- Se a regra usa regex
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100), -- Quem criou (sistema, admin, bot_detector, etc)
    detection_count INTEGER DEFAULT 0, -- Quantas vezes foi detectada
    last_detection TIMESTAMP -- Última vez que foi detectada
);

-- Índices para performance
CREATE INDEX IF NOT EXISTS idx_antispam_rules_type_active ON public.antispam_rules(rule_type, is_active);
CREATE INDEX IF NOT EXISTS idx_antispam_rules_value ON public.antispam_rules(rule_value);
CREATE INDEX IF NOT EXISTS idx_antispam_rules_created_at ON public.antispam_rules(created_at);

-- Função para atualizar updated_at automaticamente
CREATE OR REPLACE FUNCTION update_antispam_rules_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger para atualizar updated_at
DROP TRIGGER IF EXISTS trigger_update_antispam_rules_updated_at ON public.antispam_rules;
CREATE TRIGGER trigger_update_antispam_rules_updated_at
    BEFORE UPDATE ON public.antispam_rules
    FOR EACH ROW
    EXECUTE FUNCTION update_antispam_rules_updated_at();

-- Inserir algumas regras iniciais baseadas no que já temos
INSERT INTO public.antispam_rules (rule_type, rule_value, description, severity, created_by, is_regex) VALUES
-- IPs dos bots russos
('ip', '45.141.215.111', 'Bot russo detectado', 'high', 'initial_setup', false),
('ip', '103.251.167.20', 'Bot russo detectado', 'high', 'initial_setup', false),
('ip', '192.42.116.217', 'Bot russo detectado', 'high', 'initial_setup', false),
('ip', '154.41.95.2', 'Bot russo detectado', 'high', 'initial_setup', false),
('ip', '185.246.188.74', 'Bot russo detectado', 'high', 'initial_setup', false),
('ip', '45.90.185.110', 'Bot russo detectado', 'high', 'initial_setup', false),
('ip', '192.42.116.198', 'Bot russo detectado', 'high', 'initial_setup', false),
('ip', '185.40.4.150', 'Bot russo detectado', 'high', 'initial_setup', false),

-- Domínios de email descartável
('domain', '10minutemail.com', 'Email temporário', 'medium', 'initial_setup', false),
('domain', 'mailinator.com', 'Email temporário', 'medium', 'initial_setup', false),
('domain', 'guerrillamail.com', 'Email temporário', 'medium', 'initial_setup', false),
('domain', 'temp-mail.org', 'Email temporário', 'medium', 'initial_setup', false),
('domain', 'yopmail.com', 'Email temporário', 'medium', 'initial_setup', false),
('domain', 'tempmail.email', 'Email temporário', 'medium', 'initial_setup', false),

-- Padrões de email suspeitos
('email_pattern', 'no-reply@', 'Padrão de email bot', 'medium', 'initial_setup', false),
('email_pattern', 'noreply@', 'Padrão de email bot', 'medium', 'initial_setup', false),
('email_pattern', 'test@', 'Padrão de email teste', 'medium', 'initial_setup', false),
('email_pattern', 'admin@', 'Padrão de email admin', 'medium', 'initial_setup', false),
('email_pattern', 'bot@', 'Padrão de email bot', 'high', 'initial_setup', false),

-- Padrões de nome suspeitos (regex)
('name_pattern', '^test$', 'Nome teste', 'medium', 'initial_setup', true),
('name_pattern', '^bot$', 'Nome bot', 'high', 'initial_setup', true),
('name_pattern', '^spam$', 'Nome spam', 'high', 'initial_setup', true),
('name_pattern', '^admin$', 'Nome admin', 'medium', 'initial_setup', true),
('name_pattern', '^\d+$', 'Nome apenas números', 'medium', 'initial_setup', true),

-- Padrões russos específicos
('name_pattern', 'Поздравляем', 'Padrão russo spam', 'critical', 'initial_setup', false),
('name_pattern', 'Wilberries', 'Bot russo Wildberries', 'critical', 'initial_setup', false),
('name_pattern', 'выбраны для участия', 'Spam russo participação', 'critical', 'initial_setup', false),

-- User-Agents suspeitos
('user_agent', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36', 'User-Agent bot russo', 'high', 'initial_setup', false);

-- Comentários sobre os tipos de regras:
-- 'ip': Endereços IP específicos para bloquear
-- 'domain': Domínios de email para bloquear (ex: @tempmail.com)
-- 'email_pattern': Padrões que aparecem em emails (ex: no-reply@, test@)
-- 'name_pattern': Padrões que aparecem em nomes (pode ser regex se is_regex=true)
-- 'user_agent': User-Agents específicos para bloquear

SELECT 'Tabela antispam_rules criada com sucesso!' as status;
