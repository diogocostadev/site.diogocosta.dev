-- ========================================
-- SCRIPT 000 - TEST CONNECTION
-- Versão: 1.0.0
-- Data: 2024-12-20
-- Descrição: Testar conexão com PostgreSQL e criar schema inicial
-- ========================================

-- Verificar versão do PostgreSQL
SELECT version();

-- Verificar usuário conectado
SELECT current_user, current_database();

-- Verificar schemas existentes
SELECT schema_name 
FROM information_schema.schemata 
WHERE schema_name NOT IN ('information_schema', 'pg_catalog', 'pg_toast', 'pg_temp_1');

-- Testar se podemos criar objetos
DO $$
BEGIN
    -- Tentar criar um schema temporário
    CREATE SCHEMA IF NOT EXISTS test_connection;
    
    -- Criar uma tabela temporária
    CREATE TABLE IF NOT EXISTS test_connection.temp_test (
        id SERIAL PRIMARY KEY,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );
    
    -- Inserir um registro
    INSERT INTO test_connection.temp_test (id) VALUES (DEFAULT);
    
    -- Verificar se funcionou
    IF EXISTS (SELECT 1 FROM test_connection.temp_test) THEN
        RAISE NOTICE '✅ Conexão testada com sucesso!';
        RAISE NOTICE '✅ Permissões de escrita OK!';
        RAISE NOTICE '✅ Banco de dados pronto para uso!';
    END IF;
    
    -- Limpar o teste
    DROP TABLE test_connection.temp_test;
    DROP SCHEMA test_connection;
    
EXCEPTION WHEN OTHERS THEN
    RAISE NOTICE '❌ Erro ao testar conexão: %', SQLERRM;
END $$;

-- Mostrar informações do banco
SELECT 
    'Database' as info_type,
    current_database() as value
UNION ALL
SELECT 
    'User',
    current_user
UNION ALL
SELECT 
    'Host',
    inet_server_addr()::text
UNION ALL
SELECT 
    'Port',
    inet_server_port()::text
UNION ALL
SELECT 
    'Version',
    split_part(version(), ' ', 2);

-- Verificar extensões disponíveis (úteis para SaaS)
SELECT name, default_version, installed_version
FROM pg_available_extensions 
WHERE name IN ('uuid-ossp', 'pgcrypto', 'hstore', 'citext')
ORDER BY name;

-- ========================================
-- Para executar: psql -h 10.10.0.8 -U dcov3rl0rd -d leads -f 000_test_connection.sql
-- ======================================== 