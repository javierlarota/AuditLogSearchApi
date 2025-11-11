-- =============================================
-- Full-Text Search Audit Log Database Schema
-- PostgreSQL 12+
-- Supports searching across ALL columns
-- =============================================

-- Create the audit_logs table
CREATE TABLE IF NOT EXISTS audit_logs (
    id BIGSERIAL PRIMARY KEY,
    timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    user_id VARCHAR(100) NOT NULL,
    user_name VARCHAR(255),
    action VARCHAR(100) NOT NULL,
    resource_type VARCHAR(100) NOT NULL,
    resource_id VARCHAR(255),
    ip_address INET,
    status VARCHAR(50) NOT NULL,
    details TEXT,
    metadata JSONB,
    search_vector tsvector,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Enable pg_trgm extension for partial/fuzzy matching
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Create indices for individual column searches
CREATE INDEX IF NOT EXISTS idx_audit_logs_timestamp ON audit_logs(timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_user_name ON audit_logs(user_name);
CREATE INDEX IF NOT EXISTS idx_audit_logs_action ON audit_logs(action);
CREATE INDEX IF NOT EXISTS idx_audit_logs_resource_type ON audit_logs(resource_type);
CREATE INDEX IF NOT EXISTS idx_audit_logs_resource_id ON audit_logs(resource_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_status ON audit_logs(status);
CREATE INDEX IF NOT EXISTS idx_audit_logs_created_at ON audit_logs(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_audit_logs_ip_address ON audit_logs(ip_address);

-- Create GIN index for full-text search across ALL text columns (critical for performance)
CREATE INDEX IF NOT EXISTS idx_audit_logs_search_vector ON audit_logs USING GIN(search_vector);

-- Create GIN index for JSONB metadata queries
CREATE INDEX IF NOT EXISTS idx_audit_logs_metadata ON audit_logs USING GIN(metadata);

-- Create individual text search indices for specific column searches
CREATE INDEX IF NOT EXISTS idx_audit_logs_user_id_trgm ON audit_logs USING GIN(user_id gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_audit_logs_user_name_trgm ON audit_logs USING GIN(user_name gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_audit_logs_action_trgm ON audit_logs USING GIN(action gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_audit_logs_resource_type_trgm ON audit_logs USING GIN(resource_type gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_audit_logs_resource_id_trgm ON audit_logs USING GIN(resource_id gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_audit_logs_status_trgm ON audit_logs USING GIN(status gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_audit_logs_details_trgm ON audit_logs USING GIN(details gin_trgm_ops);

-- Create a function to update the search vector with ALL columns
CREATE OR REPLACE FUNCTION audit_logs_search_vector_update() 
RETURNS TRIGGER AS $$
BEGIN
    -- Combine ALL text fields into the search vector for comprehensive searching
    -- A weight = 1.0 (highest), B = 0.4, C = 0.2, D = 0.1 (lowest)
    NEW.search_vector := 
        setweight(to_tsvector('english', COALESCE(NEW.user_id, '')), 'A') ||
        setweight(to_tsvector('english', COALESCE(NEW.user_name, '')), 'A') ||
        setweight(to_tsvector('english', COALESCE(NEW.action, '')), 'A') ||
        setweight(to_tsvector('english', COALESCE(NEW.resource_type, '')), 'B') ||
        setweight(to_tsvector('english', COALESCE(NEW.resource_id, '')), 'B') ||
        setweight(to_tsvector('english', COALESCE(NEW.status, '')), 'B') ||
        setweight(to_tsvector('english', COALESCE(host(NEW.ip_address), '')), 'C') ||
        setweight(to_tsvector('english', COALESCE(NEW.details, '')), 'C') ||
        setweight(to_tsvector('english', COALESCE(NEW.metadata::text, '')), 'D');
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create trigger to automatically update search_vector on INSERT or UPDATE
DROP TRIGGER IF EXISTS trig_audit_logs_search_vector_update ON audit_logs;
CREATE TRIGGER trig_audit_logs_search_vector_update
    BEFORE INSERT OR UPDATE ON audit_logs
    FOR EACH ROW
    EXECUTE FUNCTION audit_logs_search_vector_update();

-- Create helper function for column-specific searches
CREATE OR REPLACE FUNCTION search_audit_logs_by_column(
    p_column_name TEXT,
    p_search_term TEXT
) RETURNS TABLE (
    id BIGINT,
    "timestamp" TIMESTAMPTZ,
    user_id VARCHAR(100),
    user_name VARCHAR(255),
    action VARCHAR(100),
    resource_type VARCHAR(100),
    resource_id VARCHAR(255),
    ip_address INET,
    status VARCHAR(50),
    details TEXT,
    metadata JSONB,
    created_at TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY EXECUTE format(
        'SELECT id, timestamp, user_id, user_name, action, resource_type, 
                resource_id, ip_address, status, details, metadata, created_at
         FROM audit_logs
         WHERE %I::text ILIKE $1',
        p_column_name
    ) USING '%' || p_search_term || '%';
END;
$$ LANGUAGE plpgsql;

-- Grant permissions (adjust as needed for your environment)
-- GRANT SELECT, INSERT, UPDATE ON audit_logs TO your_app_user;
-- GRANT USAGE, SELECT ON SEQUENCE audit_logs_id_seq TO your_app_user;
-- GRANT EXECUTE ON FUNCTION search_audit_logs_by_column TO your_app_user;

COMMENT ON TABLE audit_logs IS 'Audit log entries with full-text search capability across all columns';
COMMENT ON COLUMN audit_logs.search_vector IS 'Full-text search vector automatically maintained by trigger - includes ALL columns';
COMMENT ON COLUMN audit_logs.metadata IS 'Additional metadata stored as JSON - also searchable';
COMMENT ON FUNCTION search_audit_logs_by_column IS 'Helper function to search specific columns by name';
