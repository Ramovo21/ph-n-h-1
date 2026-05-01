-- =========================================================================
-- KICH BAN TEST YC4: SAO LUU VA PHUC HOI (BACKUP & RECOVERY)
-- Chay tung khoi trong SQL Developer
-- =========================================================================

-- =======================================================================
-- KB4.1: KIEM TRA CAU TRUC BACKUP DA TAO
-- Dang nhap: BVOWNER / BVOwner#2026
-- =======================================================================

-- Kiem tra directory backup
SELECT directory_name, directory_path FROM dba_directories WHERE directory_name = 'BACKUP_DIR';
-- Ket qua mong doi: BACKUP_DIR | C:\benhvien_backup

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
SELECT FILE_NAME
FROM BACKUP_LOG
WHERE BAK_TYPE = 'AUTO_EXPORT'
  AND STATUS = 'DONE'
ORDER BY BAK_ID DESC
FETCH FIRST 1 ROW ONLY;

-- Goi procedure phuc hoi (thay ten file bang file thuc te tu thu muc C:\oracle_backup)
-- Vi du: bvowner_20260331_113705.dmp
-- Luu y: thay ten file dung voi file backup cua ban
BEGIN
    PROC_IMPORT_RESTORE('BVOWNER_20260501_145258.DMP');  -- <-- THAY TEN FILE THUC TE
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





-- =======================================================================
-- KB4.5: PHUC HOI DU LIEU DUA TREN NHAT KY KIEM TOAN (FLASHBACK QUERY)
-- Dang nhap: BVOWNER / BVOwner#2026
-- =======================================================================

-- Buoc 1: Xem du lieu hien tai truoc khi thay doi
SELECT * FROM BVOWNER.DONTHUOC
WHERE MAHSBA = 'HSBA_001';

-- =======================================================================
-- Buoc 2: Thuc hien cap nhat sai (mo phong loi nghiep vu)
-- Dang nhap: BS_001 / BS@bv2026!
-- =======================================================================
UPDATE BVOWNER.DONTHUOC
SET LIEUDUNG = 'Sai lieu dung'
WHERE MAHSBA = 'HSBA_001';
COMMIT;

-- Kiem tra du lieu sau khi bi sai
SELECT * FROM BVOWNER.DONTHUOC
WHERE MAHSBA = 'HSBA_001';
-- Ket qua mong doi: LIEUDUNG = 'Sai lieu dung'

-- =======================================================================
-- Buoc 3: Lay thoi diem thay doi tu audit trail
-- Dang nhap: BVOWNER / BVOwner#2026
-- =======================================================================
SELECT TO_CHAR(event_timestamp, 'DD-MON-YY HH24:MI:SS') AS THOI_GIAN,
       dbusername,
       action_name,
       object_name,
       unified_audit_policies
FROM unified_audit_trail
WHERE object_name = 'DONTHUOC'
  AND action_name = 'UPDATE'
ORDER BY event_timestamp DESC
FETCH FIRST 5 ROWS ONLY;

-- =======================================================================
-- Buoc 4: Flashback Query - xem du lieu truoc khi bi thay doi
-- =======================================================================
SELECT * FROM BVOWNER.DONTHUOC
AS OF TIMESTAMP TIMESTAMP '2026-05-01 15:07:00'
WHERE MAHSBA = 'HSBA_001';
-- Ket qua mong doi: LIEUDUNG la gia tri dung truoc khi bi cap nhat sai

-- =======================================================================
-- Buoc 5: Phuc hoi du lieu bang tay
-- =======================================================================
-- Cap nhat lai du lieu dung dua tren ket qua flashback
-- (Ban co the copy gia tri dung tu ket qua tren)

UPDATE BVOWNER.DONTHUOC
SET LIEUDUNG = 'Uong 1 vien/ngay truoc an sang 30 phut, dung truoc 7 ngay'
WHERE MAHSBA = 'HSBA_001' AND TENTHUOC = 'Omeprazole 20mg';
COMMIT;
 
-- =======================================================================
-- Buoc 6: Xem du lieu sau khi da phuc hoi
-- =======================================================================
SELECT * FROM BVOWNER.DONTHUOC
WHERE MAHSBA = 'HSBA_001';
-- Ket qua mong doi: Du lieu quay lai nhu luc truoc khi bi thay doi sai




-- =======================================================================
-- KB4.6: PHUC HOI DU LIEU SAU KHI BI XOA TOAN BO (FLASHBACK TABLE)
-- Dang nhap: BVOWNER / BVOwner#2026
-- =======================================================================

-- =======================================================================
-- Buoc 1: Kiem tra du lieu ban dau
-- =======================================================================
SELECT COUNT(*) FROM BVOWNER.DONTHUOC;
-- Mong doi: > 0

-- =======================================================================
-- Buoc 2: Mo phong loi nghiep vu (xoa toan bo du lieu)
-- Dang nhap: BS_001 / BS@bv2026!
-- =======================================================================
DELETE FROM BVOWNER.DONTHUOC;
COMMIT;

-- Kiem tra sau khi bi xoa
SELECT COUNT(*) FROM BVOWNER.DONTHUOC;
-- Mong doi: = 0

-- =======================================================================
-- Buoc 3: Lay thoi diem truoc khi bi xoa (tu audit)
-- Dang nhap: BVOWNER / BVOwner#2026
-- =======================================================================
SELECT TO_CHAR(event_timestamp, 'DD-MON-YY HH24:MI:SS') AS THOI_GIAN,
       dbusername,
       action_name,
       object_name,
       unified_audit_policies
FROM unified_audit_trail
WHERE object_name = 'BVOWNER.DONTHUOC'
  AND action_name = 'DELETE'
ORDER BY event_timestamp DESC;

-- =======================================================================
-- Buoc 4: Flashback Table
-- =======================================================================
FLASHBACK TABLE BVOWNER.DONTHUOC
TO TIMESTAMP TIMESTAMP '2026-05-01 15:07:00';
COMMIT;

-- =======================================================================
-- Buoc 5: Kiem tra sau phuc hoi
-- =======================================================================
SELECT COUNT(*) FROM BVOWNER.DONTHUOC;
-- Mong doi: du lieu duoc khoi phuc lai day du

SELECT * FROM BVOWNER.DONTHUOC;





-- =======================================================================
-- KB4.7: PHUC HOI BANG BI DROP (FLASHBACK DROP)
-- =======================================================================

-- =======================================================================
-- Bước 1: Kiểm tra dữ liệu trước khi xảy ra sự cố
-- Đăng nhập: BVOWNER / BVOwner#2026
-- =======================================================================
SELECT COUNT(*) FROM BVOWNER.DONTHUOC;
-- Kết quả mong đợi: > 0 dòng

-- =======================================================================
-- Bước 2: Mô phỏng lỗi - drop nhầm bảng
-- Đăng nhập: BVOWNER / BVOwner#2026
-- =======================================================================
DROP TABLE DONTHUOC;

-- Kiểm tra lại
SELECT * FROM BVOWNER.DONTHUOC;
-- Kết quả mong đợi: ORA-00942: table or view does not exist


-- =======================================================================
-- Bước 3: Kiểm tra recycle bin
-- =======================================================================
SELECT original_name, object_name, droptime
FROM user_recyclebin
WHERE original_name = 'DONTHUOC';

-- Kết quả mong đợi:
-- thấy object_name dạng BIN$...

-- =======================================================================
-- Bước 4: Phục hồi bảng
-- =======================================================================
FLASHBACK TABLE DONTHUOC TO BEFORE DROP;

-- =======================================================================
-- Bước 5: Kiểm tra lại dữ liệu sau khi phục hồi
-- =======================================================================
SELECT COUNT(*) FROM BVOWNER.DONTHUOC;

-- Kết quả mong đợi: dữ liệu được khôi phục đầy đủ