-- =========================================================================
-- KICH BAN TEST YC4: SAO LUU VA PHUC HOI (BACKUP & RECOVERY)
-- Chay tung khoi trong SQL Developer
-- =========================================================================

-- =======================================================================
-- KB4.1: KIEM TRA CAU TRUC BACKUP DA TAO
-- Dang nhap: SYS AS SYSDBA
-- =======================================================================

-- Kiem tra directory backup
SELECT directory_name, directory_path FROM dba_directories WHERE directory_name = 'BACKUP_DIR';
-- Ket qua mong doi: BACKUP_DIR | C:\oracle_backup

-- Kiem tra scheduler job
SELECT owner, job_name, job_type, job_action, state, enabled, 
       TO_CHAR(last_start_date, 'DD-MON-YY HH24:MI') AS LAN_CHAY_CUOI,
       TO_CHAR(next_run_date, 'DD-MON-YY HH24:MI') AS LAN_CHAY_KE
FROM dba_scheduler_jobs
WHERE owner = 'BV_ADMIN' AND job_name = 'JOB_DAILY_BACKUP';
-- Ket qua mong doi: SCHEDULED, TRUE, repeat luc 2h sang

-- Kiem tra 2 procedures ton tai
SELECT object_name, object_type, status 
FROM dba_objects
WHERE owner = 'BVOWNER'
  AND object_name IN ('PROC_AUTO_EXPORT', 'PROC_IMPORT_RESTORE')
ORDER BY object_name;
-- Ket qua mong doi: 2 PROCEDURE, VALID


-- =======================================================================
-- KB4.2: CHAY BACKUP THU CONG
-- Dang nhap: BVOWNER / BVOwner#2026
-- =======================================================================

-- Chay backup
BEGIN
    PROC_AUTO_EXPORT;
END;
/
-- Ket qua mong doi: PL/SQL procedure successfully completed

-- Kiem tra backup log
SELECT BAK_ID, 
       TO_CHAR(BAK_TIME, 'DD-MON-YY HH24:MI:SS') AS THOI_GIAN,
       BAK_TYPE, 
       STATUS
FROM BACKUP_LOG
ORDER BY BAK_ID DESC;
-- Ket qua mong doi: 1+ dong, STATUS = DONE


-- =======================================================================
-- KB4.3: KIEM TRA FILE BACKUP DA TAO
-- Dang nhap: BVOWNER / BVOwner#2026
-- =======================================================================

-- dba_datapump_jobs chi hien thi job DANG CHAY, sau khi hoan thanh se rong -> dung BACKUP_LOG
-- Kiem tra cac file backup da tao qua BACKUP_LOG
SELECT BAK_ID,
       TO_CHAR(BAK_TIME, 'DD-MON-YY HH24:MI:SS') AS THOI_GIAN,
       BAK_TYPE,
       FILE_NAME,
       STATUS
FROM BVOWNER.BACKUP_LOG
ORDER BY BAK_ID DESC;
-- Ket qua mong doi: >= 1 dong, STATUS = DONE, FILE_NAME = bvowner_YYYYMMDD_HHMMSS.dmp


-- =======================================================================
-- KB4.4: MO PHONG PHUC HOI (RECOVERY)
-- Dang nhap: BVOWNER / BVOwner#2026
-- =======================================================================

-- Buoc 1: Ghi nhan so dong truoc khi xoa
SELECT 'NHANVIEN' AS BANG, COUNT(*) AS SO_DONG FROM NHANVIEN
UNION ALL
SELECT 'BENHNHAN', COUNT(*) FROM BENHNHAN
UNION ALL
SELECT 'HSBA', COUNT(*) FROM HSBA
UNION ALL
SELECT 'DONTHUOC', COUNT(*) FROM DONTHUOC;
-- Ghi lai so dong: NV=35, BN=10, HSBA=6, DT=6

-- Buoc 2: Lam backup truoc khi xoa
BEGIN
    PROC_AUTO_EXPORT;
END;
/

-- Buoc 3: Xoa du lieu (mo phong su co)
ALTER TABLE HSBA DISABLE CONSTRAINT FK_HSBA_BN;
DELETE FROM DONTHUOC;
DELETE FROM HSBA_DV;
DELETE FROM HSBA;
DELETE FROM BENHNHAN;
ALTER TABLE HSBA ENABLE CONSTRAINT FK_HSBA_BN;
COMMIT;

-- Buoc 4: Xac nhan da mat du lieu
SELECT 'BENHNHAN' AS BANG, COUNT(*) AS SO_DONG FROM BENHNHAN
UNION ALL
SELECT 'HSBA', COUNT(*) FROM HSBA
UNION ALL
SELECT 'DONTHUOC', COUNT(*) FROM DONTHUOC;
-- Ket qua mong doi: 0, 0, 0

-- Buoc 5: Phuc hoi tu backup
-- Tim ten file backup moi nhat trong backup log
SELECT BAK_TIME, BAK_TYPE, STATUS FROM BACKUP_LOG ORDER BY BAK_ID DESC;

-- Goi procedure phuc hoi (thay ten file bang file thuc te tu thu muc C:\oracle_backup)
-- Vi du: bvowner_20260331_113705.dmp
-- Luu y: thay ten file dung voi file backup cua ban
BEGIN
    PROC_IMPORT_RESTORE('bvowner_YYYYMMDD_HHMMSS.dmp');  -- <-- THAY TEN FILE THUC TE
END;
/

-- Buoc 6: Xac nhan du lieu da phuc hoi
SELECT 'NHANVIEN' AS BANG, COUNT(*) AS SO_DONG FROM NHANVIEN
UNION ALL
SELECT 'BENHNHAN', COUNT(*) FROM BENHNHAN
UNION ALL
SELECT 'HSBA', COUNT(*) FROM HSBA
UNION ALL
SELECT 'DONTHUOC', COUNT(*) FROM DONTHUOC;
-- Ket qua mong doi: so dong giong buoc 1

-- Kiem tra backup log ghi nhan ca export va import
SELECT BAK_ID, 
       TO_CHAR(BAK_TIME, 'HH24:MI:SS') AS GIO, 
       BAK_TYPE, 
       STATUS
FROM BACKUP_LOG
ORDER BY BAK_ID DESC;
-- Ket qua mong doi: co dong IMPORT_RESTORE voi STATUS = DONE
