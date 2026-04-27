-- =========================================================================
-- RUN_ALL.sql - Chạy tất cả scripts theo thứ tự
-- Chạy với: sqlplus sys/"NtMl1504:))"@localhost:1521/XEPDB1 as sysdba @d:\DAC_BenhVien\RUN_ALL.sql
-- =========================================================================

SPOOL d:\DAC_BenhVien\run_log.txt APPEND

PROMPT ================================================================
PROMPT  RUNNING: 00_CLEANUP.sql
PROMPT ================================================================
@d:\DAC_BenhVien\00_CLEANUP.sql

PROMPT ================================================================
PROMPT  RUNNING: 01_SCHEMA_DATA.sql
PROMPT ================================================================
@d:\DAC_BenhVien\01_SCHEMA_DATA.sql

PROMPT ================================================================
PROMPT  RUNNING: 00_BOOTSTRAP_ADMIN.sql
PROMPT ================================================================
@d:\DAC_BenhVien\00_BOOTSTRAP_ADMIN.sql

PROMPT ================================================================
PROMPT  RUNNING: YC1.sql
PROMPT ================================================================
CONNECT BV_ADMIN/"BvAdmin@2026!"@localhost:1521/XEPDB1
@d:\DAC_BenhVien\YC1.sql

PROMPT ================================================================
PROMPT  RUNNING: YC2.sql
PROMPT ================================================================
CONNECT BV_ADMIN/"BvAdmin@2026!"@localhost:1521/XEPDB1
@d:\DAC_BenhVien\YC2.sql

PROMPT ================================================================
PROMPT  RUNNING: YC3.sql
PROMPT ================================================================
CONNECT BV_ADMIN/"BvAdmin@2026!"@localhost:1521/XEPDB1
@d:\DAC_BenhVien\YC3.sql

PROMPT ================================================================
PROMPT  RUNNING: YC4.sql
PROMPT ================================================================
CONNECT BV_ADMIN/"BvAdmin@2026!"@localhost:1521/XEPDB1
@d:\DAC_BenhVien\YC4.sql

PROMPT ================================================================
PROMPT  OPTIONAL: RUN SCHEMA.sql / USER.sql / ROLE.sql IF YOU WANT TO EXECUTE IN SEPARATE STAGES
PROMPT ================================================================

SPOOL OFF
PROMPT ================================================================
PROMPT  ALL SCRIPTS COMPLETED. Check d:\DAC_BenhVien\run_log.txt
PROMPT ================================================================
EXIT
