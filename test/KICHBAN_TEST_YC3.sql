-- =========================================================================
-- KICH BAN TEST YC3: KIEM TOAN (AUDIT)
-- Chay tung khoi trong SQL Developer
-- =========================================================================

-- =======================================================================
-- KB3.1: KIEM TRA CAC AUDIT POLICY DA TAO
-- Dang nhap: BV_ADMIN / BvAdmin@2026!
-- =======================================================================

-- Liet ke tat ca unified audit policies
SELECT policy_name, audit_option, object_schema, object_name
FROM audit_unified_policies
WHERE policy_name LIKE 'AUD_POL_%'
ORDER BY policy_name;

-- Kem tra policies da duoc ENABLE
SELECT policy_name, enabled_option, entity_name, entity_type
FROM audit_unified_enabled_policies
WHERE policy_name LIKE 'AUD_POL_%'
ORDER BY policy_name;

-- Kiem tra FGA policies
SELECT policy_name, object_schema, object_name, policy_column,
       SEL, INS, UPD, DEL, enabled
FROM dba_audit_policies
WHERE object_schema = 'BVOWNER'
ORDER BY policy_name;


-- =======================================================================
-- KB3.2: TAO SU KIEN AUDIT - BS_001 TRUY CAP DU LIEU
-- Dang nhap: BS_001 / BS@bv2026!
-- =======================================================================

-- Su kien 1: SELECT benh nhan (audit boi AUD_POL_SELECT_BN)
SELECT MABN, TENBN FROM BVOWNER.BENHNHAN;

-- Su kien 2: UPDATE don thuoc (audit boi AUD_POL_DML_DT + FGA_UPDATE_DONTHUOC)
UPDATE BVOWNER.DONTHUOC SET LIEUDUNG = 'Cap nhat lieu dung moi' WHERE MAHSBA = 'HSBA_001' AND ROWNUM = 1;
ROLLBACK;

-- Su kien 3: UPDATE chan doan (audit boi AUD_POL_DML_HSBA + FGA_UPDATE_HSBA_BS)
UPDATE BVOWNER.HSBA SET CHANDOAN = 'Cap nhat: Viem man tinh' WHERE MAHSBA = 'HSBA_001';
ROLLBACK;


-- =======================================================================
-- KB3.3: TAO SU KIEN AUDIT - TRUY CAP TRAI PHEP
-- Dang nhap: KTV_001 / KTV@bv2026!
-- =======================================================================

-- KTV co gang xem HSBA (khong co quyen) -> audit ILLEGAL
SELECT * FROM BVOWNER.HSBA;
-- Ket qua mong doi: ORA-00942

-- KTV co gang xem BENHNHAN (khong co quyen) -> audit ILLEGAL
SELECT * FROM BVOWNER.BENHNHAN;
-- Ket qua mong doi: ORA-00942


-- =======================================================================
-- KB3.4: KIEM TRA UNIFIED AUDIT TRAIL
-- Dang nhap: BV_ADMIN / BvAdmin@2026!
-- =======================================================================

-- Xem toan bo su kien audit trong 1 gio qua
SELECT TO_CHAR(event_timestamp, 'HH24:MI:SS') AS GIO,
       dbusername AS USER_NAME,
       action_name AS HANH_DONG,
       object_schema AS SCHEMA,
       object_name AS DOI_TUONG,
       unified_audit_policies AS POLICY,
       return_code AS MA_LOI
FROM unified_audit_trail
WHERE event_timestamp > SYSTIMESTAMP - INTERVAL '1' HOUR
  AND unified_audit_policies IS NOT NULL
  AND unified_audit_policies LIKE 'AUD_POL_%'
ORDER BY event_timestamp DESC
FETCH FIRST 30 ROWS ONLY;

-- Ket qua mong doi: Cac dong voi:
-- BS_001 | SELECT | BENHNHAN | AUD_POL_SELECT_BN
-- BS_001 | UPDATE | DONTHUOC | AUD_POL_DML_DT
-- BS_001 | UPDATE | HSBA     | AUD_POL_DML_HSBA
-- BS_001 | LOGON  |          | AUD_POL_SESSION
-- KTV_001| LOGON  |          | AUD_POL_SESSION


-- =======================================================================
-- KB3.5: KIEM TRA FINE-GRAINED AUDIT (FGA) TRAIL
-- Dang nhap: BV_ADMIN / BvAdmin@2026!
-- =======================================================================

SELECT TO_CHAR(timestamp, 'HH24:MI:SS') AS GIO,
       db_user AS USER_NAME,
       object_name AS DOI_TUONG,
       policy_name AS POLICY,
       statement_type AS LOAI_LENH,
       sql_text AS CAU_LENH
FROM dba_fga_audit_trail
WHERE object_schema = 'BVOWNER'
  AND timestamp > SYSDATE - (1/24)
ORDER BY timestamp DESC;

-- Ket qua mong doi:
-- BS_001 | DONTHUOC | FGA_UPDATE_DONTHUOC | UPDATE | UPDATE BVOWNER.DONTHUOC SET...
-- BS_001 | HSBA     | FGA_UPDATE_HSBA_BS  | UPDATE | UPDATE BVOWNER.HSBA SET...


-- =======================================================================
-- KB3.6: KIEM TRA AUDIT SESSION (LOGON/LOGOFF)
-- Dang nhap: BV_ADMIN / BvAdmin@2026!
-- =======================================================================

SELECT TO_CHAR(event_timestamp, 'HH24:MI:SS') AS GIO,
       dbusername AS USER_NAME,
       action_name AS HANH_DONG,
       authentication_type AS LOAI_XACTHUC,
       return_code AS MA_LOI
FROM unified_audit_trail
WHERE unified_audit_policies = 'AUD_POL_SESSION'
  AND event_timestamp > SYSTIMESTAMP - INTERVAL '1' HOUR
ORDER BY event_timestamp DESC
FETCH FIRST 20 ROWS ONLY;

-- Ket qua mong doi: Cac dong LOGON/LOGOFF cua tat ca user da dang nhap
