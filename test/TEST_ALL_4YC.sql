-- =========================================================================
-- TEST_ALL_4YC.sql - Chay va test tong hop YC1..YC4
-- =========================================================================

SET ECHO ON
SET FEEDBACK ON
SET SERVEROUTPUT ON
SET PAGESIZE 200
SET LINESIZE 220

SPOOL d:\DAC_BenhVien\test_results.txt

PROMPT ==================== SETUP NEN ====================

CONNECT sys/"NtMl1504:))"@localhost:1521/XEPDB1 AS SYSDBA
@d:\DAC_BenhVien\00_CLEANUP.sql
@d:\DAC_BenhVien\01_SCHEMA_DATA.sql
@d:\DAC_BenhVien\00_BOOTSTRAP_ADMIN.sql

PROMPT ==================== YC1: CHAY + VERIFY ====================

@d:\DAC_BenhVien\YC1.sql

PROMPT --- TC#2: DPV_001 thay tat ca HSBA va BENHNHAN ---

CONNECT DPV_001/"DPV@bv2026!"@localhost:1521/XEPDB1
SELECT COUNT(*) AS DPV_HSBA_COUNT FROM BVOWNER.HSBA;
SELECT COUNT(*) AS DPV_BN_COUNT FROM BVOWNER.BENHNHAN;
SELECT MANV, HOTEN FROM BVOWNER.V_CURRENT_NHANVIEN;

PROMPT --- TC#3: BS_001 chi thay HSBA cua minh ---

CONNECT BS_001/"BS@bv2026!"@localhost:1521/XEPDB1
SELECT COUNT(*) AS BS_HSBA_VISIBLE FROM BVOWNER.HSBA;
SELECT COUNT(*) AS BS_BN_VISIBLE FROM BVOWNER.BENHNHAN;
UPDATE BVOWNER.DONTHUOC SET LIEUDUNG = LIEUDUNG WHERE ROWNUM = 1;
ROLLBACK;

PROMPT --- TC#4: KTV_001 chi thay HSBA_DV cua minh ---

CONNECT KTV_001/"KTV@bv2026!"@localhost:1521/XEPDB1
SELECT COUNT(*) AS KTV_HSBADV_VISIBLE FROM BVOWNER.V_CURRENT_HSBA_DV_KTV;
UPDATE BVOWNER.V_CURRENT_HSBA_DV_KTV SET KETQUA = KETQUA WHERE ROWNUM = 1;
ROLLBACK;

PROMPT --- TC#5: BN_001 xem + cap nhat thong tin ca nhan ---

CONNECT BN_001/"BN@bv2026!"@localhost:1521/XEPDB1
SELECT MABN, TENBN FROM BVOWNER.V_CURRENT_BENHNHAN;
UPDATE BVOWNER.V_CURRENT_BENHNHAN SET SONHA = '99' WHERE ROWNUM = 1;
ROLLBACK;

PROMPT --- VPD Policies ---

CONNECT sys/"NtMl1504:))"@localhost:1521/XEPDB1 AS SYSDBA
SELECT object_owner, object_name, policy_name FROM dba_policies WHERE object_owner='BVOWNER' AND object_name IN ('HSBA','BENHNHAN','DONTHUOC') ORDER BY object_name;

PROMPT ==================== YC2: CHAY + VERIFY ====================

@d:\DAC_BenhVien\YC2.sql

PROMPT --- OLS Policy tren THONGBAO ---

CONNECT sys/"NtMl1504:))"@localhost:1521/XEPDB1 AS SYSDBA
SELECT policy_name, schema_name, table_name FROM dba_sa_table_policies WHERE schema_name='BVOWNER' AND table_name='THONGBAO';

PROMPT --- Admin thay tat ca 7 thong bao ---

SELECT MATB, OLS_LABEL, SUBSTR(NOIDUNG,1,60) AS NOIDUNG FROM BVOWNER.THONGBAO ORDER BY MATB;

PROMPT --- OLS TEST: BS_001 (NV:TH:HCM) ---

CONNECT BS_001/"BS@bv2026!"@localhost:1521/XEPDB1
SELECT MATB, SUBSTR(NOIDUNG,1,60) AS NOIDUNG FROM BVOWNER.THONGBAO ORDER BY MATB;

PROMPT --- OLS TEST: BS_002 (NV:TK:HN) ---

CONNECT BS_002/"BS@bv2026!"@localhost:1521/XEPDB1
SELECT MATB, SUBSTR(NOIDUNG,1,60) AS NOIDUNG FROM BVOWNER.THONGBAO ORDER BY MATB;

PROMPT --- OLS TEST: BS_007 (NV:TH:HN) ---

CONNECT BS_007/"BS@bv2026!"@localhost:1521/XEPDB1
SELECT MATB, SUBSTR(NOIDUNG,1,60) AS NOIDUNG FROM BVOWNER.THONGBAO ORDER BY MATB;

PROMPT --- OLS TEST: KTV_001 (NV::HCM) ---

CONNECT KTV_001/"KTV@bv2026!"@localhost:1521/XEPDB1
SELECT MATB, SUBSTR(NOIDUNG,1,60) AS NOIDUNG FROM BVOWNER.THONGBAO ORDER BY MATB;

PROMPT ==================== YC3: CHAY + VERIFY ====================

@d:\DAC_BenhVien\YC3.sql

PROMPT --- Tao du lieu audit ---

CONNECT BS_001/"BS@bv2026!"@localhost:1521/XEPDB1
UPDATE BVOWNER.DONTHUOC SET LIEUDUNG = LIEUDUNG WHERE ROWNUM = 1;
ROLLBACK;

PROMPT --- Unified Audit Trail ---

CONNECT sys/"NtMl1504:))"@localhost:1521/XEPDB1 AS SYSDBA
SELECT event_timestamp, dbusername, action_name, object_name, unified_audit_policies FROM unified_audit_trail WHERE event_timestamp > SYSTIMESTAMP - INTERVAL '30' MINUTE AND unified_audit_policies IS NOT NULL ORDER BY event_timestamp DESC FETCH FIRST 15 ROWS ONLY;

PROMPT ==================== YC4: CHAY + VERIFY ====================

@d:\DAC_BenhVien\YC4.sql

PROMPT --- Test Backup ---

CONNECT BVOWNER/"BVOwner#2026"@localhost:1521/XEPDB1
BEGIN PROC_AUTO_EXPORT; END;
/

PROMPT --- Backup Log ---

SELECT BAK_ID, BAK_TIME, BAK_TYPE, STATUS FROM BACKUP_LOG ORDER BY BAK_ID DESC FETCH FIRST 5 ROWS ONLY;

PROMPT --- Scheduler Job ---

CONNECT sys/"NtMl1504:))"@localhost:1521/XEPDB1 AS SYSDBA
SELECT owner, job_name, state, enabled FROM dba_scheduler_jobs WHERE owner='BVOWNER' AND job_name='JOB_DAILY_BACKUP';

PROMPT --- Recovery Procedures ---

SELECT object_name, object_type, status FROM dba_objects WHERE owner='BVOWNER' AND object_name IN ('PROC_AUTO_EXPORT','PROC_IMPORT_RESTORE');

SPOOL OFF
PROMPT ==================== DONE TEST YC1..YC4 ====================
