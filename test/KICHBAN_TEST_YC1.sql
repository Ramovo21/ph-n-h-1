-- =========================================================================
-- KICH BAN TEST YC1: PHAN QUYEN TRUY CAP (RBAC + VPD)
-- Chay tung khoi trong SQL Developer
-- =========================================================================

-- =======================================================================
-- KB1.1: KIEM TRA TC#1 - MOI NHAN VIEN/BENH NHAN CO 1 ORACLE ACCOUNT
-- Dang nhap: BV_ADMIN / BvAdmin@2026!
-- =======================================================================

-- Dem so user Oracle duoc tao tu bang NHANVIEN
SELECT 'Nhan vien co Oracle account' AS LOAI, COUNT(*) AS SO_LUONG
FROM BVOWNER.NHANVIEN WHERE ORA_USERNAME IS NOT NULL;

-- Dem so user Oracle duoc tao tu bang BENHNHAN
SELECT 'Benh nhan co Oracle account' AS LOAI, COUNT(*) AS SO_LUONG
FROM BVOWNER.BENHNHAN WHERE ORA_USERNAME IS NOT NULL;

-- Kiem tra cac user da duoc tao trong Oracle
SELECT username, account_status, created
FROM dba_users
WHERE username LIKE 'DPV_%' OR username LIKE 'BS_%'
   OR username LIKE 'KTV_%' OR username LIKE 'BN_%'
ORDER BY username;

-- Kiem tra roles da duoc tao
SELECT role FROM dba_roles
WHERE role IN ('ROLE_DIEUPHOI','ROLE_BACSI','ROLE_KTV','ROLE_BENHNHAN');

-- Kiem tra VPD policies da duoc tao
SELECT object_owner, object_name, policy_name, function, sel, upd, del
FROM dba_policies
WHERE object_owner = 'BVOWNER'
ORDER BY object_name;


-- =======================================================================
-- KB1.2: KIEM TRA TC#2 - DIEU PHOI VIEN (DPV)
-- Dang nhap: DPV_001 / DPV@bv2026!
-- =======================================================================

-- DPV thay TAT CA ho so benh an (VPD cho phep 1=1)
SELECT COUNT(*) AS TONG_HSBA FROM BVOWNER.HSBA;
-- Ket qua mong doi: 6

-- DPV thay TAT CA benh nhan
SELECT COUNT(*) AS TONG_BENHNHAN FROM BVOWNER.BENHNHAN;
-- Ket qua mong doi: 10

-- DPV xem thong tin ca nhan cua minh
SELECT MANV, HOTEN, VAITRO, CHUYENKHOA FROM BVOWNER.V_CURRENT_NHANVIEN;
-- Ket qua mong doi: chi 1 dong - NV_DPV_001

-- DPV cap nhat so dien thoai ca nhan
UPDATE BVOWNER.V_CURRENT_NHANVIEN_UPDATABLE SET SODT = '0909999999' WHERE ROWNUM = 1;
SELECT MANV, SODT FROM BVOWNER.V_CURRENT_NHANVIEN;
-- Ket qua mong doi: SODT = 0909999999
ROLLBACK;

-- DPV them benh nhan moi
INSERT INTO BVOWNER.BENHNHAN (MABN, TENBN, PHAI, NGAYSINH, CCCD, SONHA, TENDUONG, QUANHUYEN, TINHTP)
VALUES ('BN_TEST','Nguyen Van Test','Nam',TO_DATE('2000-01-01','YYYY-MM-DD'),'399999999','1','Test','Q1','TP HCM');
-- Ket qua mong doi: 1 row inserted
ROLLBACK;

-- DPV them ho so benh an moi
INSERT INTO BVOWNER.HSBA (MAHSBA, MABN, NGAY, CHANDOAN, MABS, MAKHOA)
VALUES ('HSBA_TEST','BN_001',SYSDATE,'Test chandoan','NV_BS_001','Tieu hoa');
-- Ket qua mong doi: 1 row inserted
ROLLBACK;

-- DPV KHONG duoc xem DONTHUOC (khong co quyen)
SELECT * FROM BVOWNER.DONTHUOC;
-- Ket qua mong doi: ORA-00942 table or view does not exist


-- =======================================================================
-- KB1.3: KIEM TRA TC#3 - BAC SI / Y SI (BS)
-- Dang nhap: BS_001 / BS@bv2026!
-- =======================================================================

-- BS_001 chi thay HSBA minh phu trach (MABS = NV_BS_001)
SELECT MAHSBA, MABN, CHANDOAN, MABS FROM BVOWNER.HSBA;
-- Ket qua mong doi: 2 dong (HSBA_001 va HSBA_004, ca 2 co MABS = NV_BS_001)

-- BS_001 chi thay benh nhan lien quan qua HSBA
SELECT MABN, TENBN FROM BVOWNER.BENHNHAN;
-- Ket qua mong doi: 2 dong (BN_001 va BN_004)

-- BS_001 cap nhat chan doan
UPDATE BVOWNER.HSBA SET CHANDOAN = 'Cap nhat: Viem da day man tinh' WHERE MAHSBA = 'HSBA_001';
-- Ket qua mong doi: 1 row updated
ROLLBACK;

-- BS_001 cap nhat tien su benh cua benh nhan
UPDATE BVOWNER.BENHNHAN SET TIENSUBENH = 'Cap nhat tien su' WHERE MABN = 'BN_001';
-- Ket qua mong doi: 1 row updated
ROLLBACK;

-- BS_001 xem don thuoc cua minh
SELECT MAHSBA, TENTHUOC, LIEUDUNG FROM BVOWNER.DONTHUOC;
-- Ket qua mong doi: 3 dong (2 don cua HSBA_001 + 1 don cua HSBA_004)

-- BS_001 them don thuoc moi
INSERT INTO BVOWNER.DONTHUOC VALUES ('HSBA_001',SYSDATE,'Thuoc test','Uong 1 vien/ngay');
-- Ket qua mong doi: 1 row inserted
ROLLBACK;

-- BS_001 KHONG the xem HSBA cua BS khac
SELECT * FROM BVOWNER.HSBA WHERE MABS = 'NV_BS_002';
-- Ket qua mong doi: 0 dong (VPD chan)

-- BS_001 KHONG the cap nhat BN khong thuoc minh
UPDATE BVOWNER.BENHNHAN SET TIENSUBENH = 'Hack' WHERE MABN = 'BN_002';
-- Ket qua mong doi: 0 rows updated (VPD chan)


-- =======================================================================
-- KB1.4: KIEM TRA THEM VOI BS_003 (Tim mach, Da Nang)
-- Dang nhap: BS_003 / BS@bv2026!
-- =======================================================================

-- BS_003 chi thay HSBA minh phu trach
SELECT MAHSBA, MABN, CHANDOAN, MABS FROM BVOWNER.HSBA;
-- Ket qua mong doi: 2 dong (HSBA_003 va HSBA_005, MABS = NV_BS_003)

-- BS_003 KHONG thay benh nhan cua BS_001
SELECT * FROM BVOWNER.BENHNHAN WHERE MABN = 'BN_001';
-- Ket qua mong doi: 0 dong


-- =======================================================================
-- KB1.5: KIEM TRA TC#4 - KY THUAT VIEN (KTV)
-- Dang nhap: KTV_001 / KTV@bv2026!
-- =======================================================================

-- KTV_001 chi thay dich vu minh phu trach
SELECT MAHSBA, LOAIDV, NGAYDV, KETQUA FROM BVOWNER.V_CURRENT_HSBA_DV_KTV;
-- Ket qua mong doi: 2 dong (Xet nghiem mau HSBA_001 + Noi soi da day HSBA_004)

-- KTV_001 cap nhat ket qua xet nghiem
UPDATE BVOWNER.V_CURRENT_HSBA_DV_KTV SET KETQUA = 'Ket qua moi: Bach cau binh thuong' WHERE MAHSBA = 'HSBA_001';
-- Ket qua mong doi: 1 row updated
ROLLBACK;

-- KTV_001 xem thong tin ca nhan
SELECT MANV, HOTEN, CHUYENKHOA FROM BVOWNER.V_CURRENT_NHANVIEN;
-- Ket qua mong doi: 1 dong - NV_KTV_001

-- KTV_001 KHONG duoc xem truc tiep HSBA
SELECT * FROM BVOWNER.HSBA;
-- Ket qua mong doi: ORA-00942 (khong co quyen SELECT tren HSBA)

-- KTV_001 KHONG thay dich vu cua KTV khac
-- (View V_CURRENT_HSBA_DV_KTV da loc theo MAKTV)
SELECT COUNT(*) AS SO_DV FROM BVOWNER.V_CURRENT_HSBA_DV_KTV;
-- Ket qua mong doi: 2 (chi cua minh, khong phai tat ca 6)


-- =======================================================================
-- KB1.6: KIEM TRA TC#5 - BENH NHAN (BN)
-- Dang nhap: BN_001 / BN@bv2026!
-- =======================================================================

-- BN_001 chi thay thong tin cua chinh minh
SELECT MABN, TENBN, PHAI, SONHA, TENDUONG, TINHTP FROM BVOWNER.V_CURRENT_BENHNHAN;
-- Ket qua mong doi: 1 dong - BN_001 Nguyen Thi Anh

-- BN_001 cap nhat dia chi
UPDATE BVOWNER.V_CURRENT_BENHNHAN SET SONHA = '99', TENDUONG = 'Nguyen Hue moi' WHERE MABN = 'BN_001';
-- Ket qua mong doi: 1 row updated
SELECT SONHA, TENDUONG FROM BVOWNER.V_CURRENT_BENHNHAN;
-- Ket qua mong doi: 99, Nguyen Hue moi
ROLLBACK;

-- BN_001 KHONG duoc xem ho so benh an
SELECT * FROM BVOWNER.HSBA;
-- Ket qua mong doi: ORA-00942 (khong co quyen)

-- BN_001 KHONG duoc xem thong tin benh nhan khac
SELECT * FROM BVOWNER.V_CURRENT_BENHNHAN WHERE MABN = 'BN_002';
-- Ket qua mong doi: 0 dong (view chi loc cua minh)


-- =======================================================================
-- KB1.7: KIEM TRA THEM VOI BN_005
-- Dang nhap: BN_005 / BN@bv2026!
-- =======================================================================

-- BN_005 chi thay chinh minh
SELECT MABN, TENBN FROM BVOWNER.V_CURRENT_BENHNHAN;
-- Ket qua mong doi: 1 dong - BN_005 Hoang Thi Em

-- BN_005 KHONG thay BN_001
SELECT * FROM BVOWNER.V_CURRENT_BENHNHAN WHERE MABN = 'BN_001';
-- Ket qua mong doi: 0 dong
