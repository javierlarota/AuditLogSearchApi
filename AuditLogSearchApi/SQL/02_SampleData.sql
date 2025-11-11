-- =============================================
-- Sample Data for Audit Log Testing
-- This script populates the audit_logs table with test data
-- =============================================

-- Insert sample audit log entries with diverse data for testing

INSERT INTO audit_logs (timestamp, user_id, user_name, action, resource_type, resource_id, ip_address, status, details, metadata) VALUES
-- User authentication events
('2024-01-15 08:30:00+00', 'user001', 'John Smith', 'LOGIN', 'Authentication', 'session-12345', '192.168.1.100', 'SUCCESS', 'User logged in successfully from web application', '{"browser": "Chrome", "os": "Windows", "device": "desktop"}'::jsonb),
('2024-01-15 08:35:00+00', 'user002', 'Jane Doe', 'LOGIN', 'Authentication', 'session-12346', '192.168.1.101', 'SUCCESS', 'User logged in successfully from mobile app', '{"browser": "Safari", "os": "iOS", "device": "mobile"}'::jsonb),
('2024-01-15 08:40:00+00', 'user003', 'Bob Johnson', 'LOGIN', 'Authentication', 'session-12347', '10.0.0.50', 'FAILED', 'Invalid credentials provided', '{"browser": "Firefox", "os": "Linux", "device": "desktop", "reason": "invalid_password"}'::jsonb),

-- Document operations
('2024-01-15 09:00:00+00', 'user001', 'John Smith', 'CREATE', 'Document', 'doc-2024-001', '192.168.1.100', 'SUCCESS', 'Created new financial report document', '{"documentType": "report", "category": "financial", "size": "2.5MB"}'::jsonb),
('2024-01-15 09:15:00+00', 'user002', 'Jane Doe', 'UPDATE', 'Document', 'doc-2024-001', '192.168.1.101', 'SUCCESS', 'Updated financial report with Q4 data', '{"documentType": "report", "category": "financial", "changes": ["section3", "appendix"]}'::jsonb),
('2024-01-15 09:30:00+00', 'user001', 'John Smith', 'DELETE', 'Document', 'doc-2023-999', '192.168.1.100', 'SUCCESS', 'Deleted obsolete draft document', '{"documentType": "draft", "category": "archived", "reason": "expired"}'::jsonb),
('2024-01-15 10:00:00+00', 'user004', 'Alice Williams', 'VIEW', 'Document', 'doc-2024-001', '192.168.1.102', 'SUCCESS', 'Viewed financial report for review', '{"documentType": "report", "category": "financial", "viewDuration": "15min"}'::jsonb),

-- Database operations
('2024-01-15 10:30:00+00', 'admin001', 'System Admin', 'BACKUP', 'Database', 'db-prod-001', '10.0.0.10', 'SUCCESS', 'Scheduled database backup completed successfully', '{"backupType": "full", "size": "45GB", "duration": "2.5h"}'::jsonb),
('2024-01-15 11:00:00+00', 'admin001', 'System Admin', 'RESTORE', 'Database', 'db-staging-001', '10.0.0.10', 'SUCCESS', 'Database restored from backup for testing', '{"backupType": "full", "sourceDate": "2024-01-14", "duration": "1.2h"}'::jsonb),
('2024-01-15 11:30:00+00', 'dba001', 'Database Admin', 'OPTIMIZE', 'Database', 'db-prod-001', '10.0.0.11', 'SUCCESS', 'Database optimization and indexing completed', '{"operation": "reindex", "tablesAffected": 127, "duration": "45min"}'::jsonb),

-- User management
('2024-01-15 12:00:00+00', 'admin002', 'HR Admin', 'CREATE', 'User', 'user005', '192.168.1.103', 'SUCCESS', 'New employee account created', '{"department": "Engineering", "role": "Developer", "accessLevel": "standard"}'::jsonb),
('2024-01-15 12:15:00+00', 'admin002', 'HR Admin', 'UPDATE', 'User', 'user002', '192.168.1.103', 'SUCCESS', 'Updated user role and permissions', '{"department": "Management", "role": "Manager", "accessLevel": "elevated", "changes": ["role", "permissions"]}'::jsonb),
('2024-01-15 12:30:00+00', 'admin002', 'HR Admin', 'DISABLE', 'User', 'user099', '192.168.1.103', 'SUCCESS', 'Disabled account for terminated employee', '{"department": "Sales", "reason": "termination", "effectiveDate": "2024-01-15"}'::jsonb),

-- API operations
('2024-01-15 13:00:00+00', 'api_client_001', 'External API Client', 'API_CALL', 'API', 'endpoint-users-list', '203.0.113.50', 'SUCCESS', 'Retrieved user list via API', '{"endpoint": "/api/v1/users", "method": "GET", "responseTime": "120ms", "recordCount": 150}'::jsonb),
('2024-01-15 13:05:00+00', 'api_client_002', 'Mobile App', 'API_CALL', 'API', 'endpoint-data-sync', '203.0.113.51', 'SUCCESS', 'Data synchronization completed', '{"endpoint": "/api/v1/sync", "method": "POST", "responseTime": "850ms", "recordsSynced": 45}'::jsonb),
('2024-01-15 13:10:00+00', 'api_client_003', 'Third Party Integration', 'API_CALL', 'API', 'endpoint-webhook', '198.51.100.10', 'FAILED', 'API rate limit exceeded', '{"endpoint": "/api/v1/webhook", "method": "POST", "error": "rate_limit_exceeded", "retryAfter": "60s"}'::jsonb),

-- Security events
('2024-01-15 14:00:00+00', 'security_scanner', 'Security System', 'SCAN', 'Security', 'vulnerability-scan-001', '10.0.0.20', 'SUCCESS', 'Security vulnerability scan completed', '{"scanType": "full", "vulnerabilitiesFound": 3, "severity": "low", "duration": "30min"}'::jsonb),
('2024-01-15 14:30:00+00', 'user006', 'Charlie Brown', 'LOGIN', 'Authentication', 'session-12350', '198.51.100.99', 'FAILED', 'Multiple failed login attempts detected', '{"attempts": 5, "browser": "Unknown", "blocked": true, "reason": "suspicious_activity"}'::jsonb),
('2024-01-15 15:00:00+00', 'admin001', 'System Admin', 'PATCH', 'System', 'server-prod-web-01', '10.0.0.10', 'SUCCESS', 'Security patches applied to production server', '{"patches": ["CVE-2024-0001", "CVE-2024-0002"], "rebootRequired": true, "downtime": "5min"}'::jsonb),

-- File operations
('2024-01-15 15:30:00+00', 'user001', 'John Smith', 'UPLOAD', 'File', 'file-invoice-2024-001.pdf', '192.168.1.100', 'SUCCESS', 'Uploaded invoice document to file storage', '{"fileType": "pdf", "size": "1.2MB", "category": "invoices", "encrypted": true}'::jsonb),
('2024-01-15 16:00:00+00', 'user004', 'Alice Williams', 'DOWNLOAD', 'File', 'file-invoice-2024-001.pdf', '192.168.1.102', 'SUCCESS', 'Downloaded invoice for accounting review', '{"fileType": "pdf", "size": "1.2MB", "category": "invoices"}'::jsonb),
('2024-01-15 16:30:00+00', 'user007', 'David Lee', 'SHARE', 'File', 'file-proposal-2024-002.docx', '192.168.1.104', 'SUCCESS', 'Shared proposal document with external partner', '{"fileType": "docx", "sharedWith": "partner@external.com", "expiresIn": "7days", "permissions": "view"}'::jsonb),

-- Configuration changes
('2024-01-15 17:00:00+00', 'admin001', 'System Admin', 'CONFIGURE', 'System', 'config-email-settings', '10.0.0.10', 'SUCCESS', 'Updated email notification settings', '{"module": "notifications", "changes": ["smtp_server", "port", "encryption"], "appliedBy": "admin001"}'::jsonb),
('2024-01-15 17:30:00+00', 'admin003', 'Network Admin', 'CONFIGURE', 'Network', 'config-firewall-rules', '10.0.0.12', 'SUCCESS', 'Modified firewall rules for new application', '{"rulesAdded": 3, "rulesModified": 2, "protocol": "TCP", "ports": [8080, 8443, 9000]}'::jsonb),

-- Batch operations
('2024-01-15 18:00:00+00', 'batch_processor', 'Batch System', 'BATCH_PROCESS', 'BatchJob', 'job-daily-report-001', '10.0.0.30', 'SUCCESS', 'Daily report generation batch job completed', '{"recordsProcessed": 10000, "duration": "25min", "outputFiles": 5, "emailsSent": 15}'::jsonb),
('2024-01-15 18:30:00+00', 'batch_processor', 'Batch System', 'BATCH_PROCESS', 'BatchJob', 'job-data-cleanup-001', '10.0.0.30', 'SUCCESS', 'Old data cleanup job completed', '{"recordsDeleted": 5000, "tablesAffected": 8, "duration": "15min", "spaceReclaimed": "2.5GB"}'::jsonb),

-- Monitoring and alerts
('2024-01-15 19:00:00+00', 'monitoring_system', 'Monitoring Agent', 'ALERT', 'System', 'alert-cpu-high-001', '10.0.0.40', 'WARNING', 'High CPU usage detected on production server', '{"server": "prod-web-01", "cpuUsage": 85, "threshold": 80, "duration": "10min"}'::jsonb),
('2024-01-15 19:15:00+00', 'monitoring_system', 'Monitoring Agent', 'ALERT', 'System', 'alert-disk-space-001', '10.0.0.40', 'WARNING', 'Low disk space on database server', '{"server": "prod-db-01", "diskUsage": 92, "threshold": 90, "availableSpace": "8GB"}'::jsonb),

-- Logout events
('2024-01-15 20:00:00+00', 'user001', 'John Smith', 'LOGOUT', 'Authentication', 'session-12345', '192.168.1.100', 'SUCCESS', 'User logged out successfully', '{"sessionDuration": "11.5h", "browser": "Chrome"}'::jsonb),
('2024-01-15 20:30:00+00', 'user002', 'Jane Doe', 'LOGOUT', 'Authentication', 'session-12346', '192.168.1.101', 'SUCCESS', 'User logged out from mobile app', '{"sessionDuration": "12h", "browser": "Safari", "device": "mobile"}'::jsonb);

-- Display summary of inserted data
SELECT 
    COUNT(*) as total_records,
    COUNT(DISTINCT user_id) as unique_users,
    COUNT(DISTINCT action) as unique_actions,
    COUNT(DISTINCT resource_type) as unique_resource_types,
    COUNT(CASE WHEN status = 'SUCCESS' THEN 1 END) as successful_operations,
    COUNT(CASE WHEN status = 'FAILED' THEN 1 END) as failed_operations,
    COUNT(CASE WHEN status = 'WARNING' THEN 1 END) as warning_operations
FROM audit_logs;

-- Show sample of the data by action type
SELECT action, COUNT(*) as count
FROM audit_logs
GROUP BY action
ORDER BY count DESC;
