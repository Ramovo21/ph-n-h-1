-- =========================================================================
-- 01_SCHEMA_DATA.sql - TẠO SCHEMA VÀ DỮ LIỆU MẪU
    -- =========================================================================
    -- 01_SCHEMA_DATA.sql - TẠO SCHEMA VÀ DỮ LIỆU MẪU
    -- CSC12001 - An toàn bảo mật dữ liệu - Đồ án 2
    -- Chạy với SYS AS SYSDBA trên XEPDB1
    -- =========================================================================

    -- =========================================================================
    -- BƯỚC 1: Tạo schema owner BVOWNER
    -- =========================================================================
    CREATE USER BVOWNER IDENTIFIED BY BVOwner#2026
        DEFAULT TABLESPACE USERS
        TEMPORARY TABLESPACE TEMP
        QUOTA UNLIMITED ON USERS;

    GRANT CONNECT, RESOURCE, CREATE VIEW, CREATE PROCEDURE,
        CREATE SEQUENCE, CREATE TRIGGER, CREATE SESSION TO BVOWNER;

    -- Grant để tạo VPD policies
    GRANT EXECUTE ON DBMS_RLS TO BVOWNER;

    -- Grant để đọc DBA views (cần cho TC#1)
    GRANT SELECT ON DBA_USERS TO BVOWNER;

    -- =========================================================================
    -- BƯỚC 2: Tạo các bảng chính (chạy với BVOWNER)
    -- =========================================================================
    CONNECT BVOWNER/BVOwner#2026@localhost:1521/XEPDB1

    -- BẢNG NHÂN VIÊN
    CREATE TABLE NHANVIEN (
        MANV        VARCHAR2(20)    PRIMARY KEY,
        HOTEN       NVARCHAR2(100)  NOT NULL,
        PHAI        NCHAR(3),                               -- 'Nam' / 'Nu'
        NGAYSINH    DATE,
        CMND        VARCHAR2(20)    UNIQUE,
        QUEQUAN     NVARCHAR2(200),
        SODT        VARCHAR2(15),
        VAITRO      NVARCHAR2(30),                          -- 'Dieu phoi vien','Bac si/Y si','Ky thuat vien'
        CHUYENKHOA  NVARCHAR2(50),
        COSO        NVARCHAR2(50),                          -- Co so lam viec: 'TP HCM','Ha Noi','Hai Phong'
        ORA_USERNAME VARCHAR2(30)   UNIQUE
    );

    -- BẢNG BỆNH NHÂN
    CREATE TABLE BENHNHAN (
        MABN            VARCHAR2(20)    PRIMARY KEY,
        TENBN           NVARCHAR2(100)  NOT NULL,
        PHAI            NCHAR(3),
        NGAYSINH        DATE,
        CCCD            VARCHAR2(20)    UNIQUE,
        SONHA           NVARCHAR2(50),
        TENDUONG        NVARCHAR2(100),
        QUANHUYEN       NVARCHAR2(100),
        TINHTP          NVARCHAR2(50),
        TIENSUBENH      NCLOB,
        TIENSUBENHGD    NCLOB,
        DIUNGTUOC       NVARCHAR2(500),
        -- Tên đăng nhập Oracle (kết nối TC#1)
        ORA_USERNAME    VARCHAR2(30)    UNIQUE
    );

    -- BẢNG HỒ SƠ BỆNH ÁN
    CREATE TABLE HSBA (
        MAHSBA      VARCHAR2(20)    PRIMARY KEY,
        MABN        VARCHAR2(20)    NOT NULL,
        NGAY        DATE            NOT NULL,
        CHANDOAN    NCLOB,
        DIEUTRI     NCLOB,
        MABS        VARCHAR2(20),                           -- FK -> NHANVIEN
        MAKHOA      NVARCHAR2(50),
        KETLUAN     NCLOB,
        CONSTRAINT FK_HSBA_BN   FOREIGN KEY (MABN)  REFERENCES BENHNHAN(MABN),
        CONSTRAINT FK_HSBA_BS   FOREIGN KEY (MABS)  REFERENCES NHANVIEN(MANV)
    );

    -- BẢNG DỊCH VỤ HỖ TRỢ CHẨN ĐOÁN
    CREATE TABLE HSBA_DV (
        MAHSBA      VARCHAR2(20)    NOT NULL,
        LOAIDV      NVARCHAR2(100)  NOT NULL,
        NGAYDV      DATE            NOT NULL,
        MAKTV       VARCHAR2(20),                           -- FK -> NHANVIEN (kỹ thuật viên)
        KETQUA      NCLOB,
        CONSTRAINT PK_HSBADV    PRIMARY KEY (MAHSBA, LOAIDV, NGAYDV),
        CONSTRAINT FK_HSBADV_HSBA FOREIGN KEY (MAHSBA)  REFERENCES HSBA(MAHSBA),
        CONSTRAINT FK_HSBADV_KTV  FOREIGN KEY (MAKTV)   REFERENCES NHANVIEN(MANV)
    );

    -- BẢNG ĐƠN THUỐC
    CREATE TABLE DONTHUOC (
        MAHSBA      VARCHAR2(20)    NOT NULL,
        NGAYDT      DATE            NOT NULL,
        TENTHUOC    NVARCHAR2(200)  NOT NULL,
        LIEUDUNG    NVARCHAR2(500),
        CONSTRAINT PK_DT    PRIMARY KEY (MAHSBA, NGAYDT, TENTHUOC),
        CONSTRAINT FK_DT_HSBA FOREIGN KEY (MAHSBA) REFERENCES HSBA(MAHSBA)
    );

    -- BẢNG THÔNG BÁO (cho Yêu cầu 2 - OLS)
    CREATE TABLE THONGBAO (
        MATB        VARCHAR2(20)    PRIMARY KEY,
        NOIDUNG     NCLOB,
        NGAYGIO     TIMESTAMP       DEFAULT SYSTIMESTAMP,
        DIADIEM     NVARCHAR2(200)
    );

    COMMIT;

    -- =========================================================================
    -- BƯỚC 3: Tạo Sequences
    -- =========================================================================
    CREATE SEQUENCE SEQ_MANV    START WITH 1 INCREMENT BY 1 NOCACHE;
    CREATE SEQUENCE SEQ_MABN    START WITH 1 INCREMENT BY 1 NOCACHE;
    CREATE SEQUENCE SEQ_MAHSBA  START WITH 1 INCREMENT BY 1 NOCACHE;
    CREATE SEQUENCE SEQ_MATB    START WITH 1 INCREMENT BY 1 NOCACHE;

    -- =========================================================================
    -- BƯỚC 4: Nhập dữ liệu mẫu
    -- =========================================================================

    -- 4.1 NHÂN VIÊN
    -- 20 Điều phối viên
    INSERT INTO NHANVIEN VALUES ('NV_DPV_001','Nguyen Thi Lan','Nu',TO_DATE('1985-03-10','YYYY-MM-DD'),'201000001','TP HCM','0901000001','Dieu phoi vien','Tieu hoa','TP HCM','DPV_001');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_002','Tran Van Minh','Nam',TO_DATE('1988-07-22','YYYY-MM-DD'),'201000002','Ha Noi','0901000002','Dieu phoi vien','Than kinh','Ha Noi','DPV_002');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_003','Le Thi Hoa','Nu',TO_DATE('1990-01-15','YYYY-MM-DD'),'201000003','Da Nang','0901000003','Dieu phoi vien','Tim mach','TP HCM','DPV_003');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_004','Pham Quoc Hung','Nam',TO_DATE('1987-11-05','YYYY-MM-DD'),'201000004','Hai Phong','0901000004','Dieu phoi vien','Tieu hoa','Hai Phong','DPV_004');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_005','Hoang Thi Mai','Nu',TO_DATE('1992-06-18','YYYY-MM-DD'),'201000005','Can Tho','0901000005','Dieu phoi vien','Than kinh','TP HCM','DPV_005');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_006','Vu Thi Thu','Nu',TO_DATE('1986-09-25','YYYY-MM-DD'),'201000006','TP HCM','0901000006','Dieu phoi vien','Tim mach','TP HCM','DPV_006');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_007','Dang Van Nam','Nam',TO_DATE('1989-04-08','YYYY-MM-DD'),'201000007','Ha Noi','0901000007','Dieu phoi vien','Tieu hoa','Ha Noi','DPV_007');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_008','Bui Thi Nga','Nu',TO_DATE('1991-12-30','YYYY-MM-DD'),'201000008','Hue','0901000008','Dieu phoi vien','Than kinh','Ha Noi','DPV_008');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_009','Do Van Duc','Nam',TO_DATE('1984-02-14','YYYY-MM-DD'),'201000009','Vinh','0901000009','Dieu phoi vien','Tim mach','Ha Noi','DPV_009');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_010','Nguyen Van Thanh','Nam',TO_DATE('1993-08-20','YYYY-MM-DD'),'201000010','Nha Trang','0901000010','Dieu phoi vien','Tieu hoa','TP HCM','DPV_010');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_011','Tran Thi Bich','Nu',TO_DATE('1985-05-12','YYYY-MM-DD'),'201000011','TP HCM','0901000011','Dieu phoi vien','Than kinh','TP HCM','DPV_011');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_012','Le Van Kien','Nam',TO_DATE('1987-10-03','YYYY-MM-DD'),'201000012','Ha Noi','0901000012','Dieu phoi vien','Tim mach','Ha Noi','DPV_012');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_013','Pham Thi Thuy','Nu',TO_DATE('1990-07-27','YYYY-MM-DD'),'201000013','Hai Phong','0901000013','Dieu phoi vien','Tieu hoa','Hai Phong','DPV_013');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_014','Hoang Van Long','Nam',TO_DATE('1988-03-16','YYYY-MM-DD'),'201000014','Da Nang','0901000014','Dieu phoi vien','Than kinh','TP HCM','DPV_014');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_015','Vu Van Toan','Nam',TO_DATE('1986-01-09','YYYY-MM-DD'),'201000015','TP HCM','0901000015','Dieu phoi vien','Tim mach','TP HCM','DPV_015');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_016','Dang Thi Lan','Nu',TO_DATE('1991-11-11','YYYY-MM-DD'),'201000016','Ha Noi','0901000016','Dieu phoi vien','Tieu hoa','Ha Noi','DPV_016');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_017','Bui Van Hai','Nam',TO_DATE('1989-09-04','YYYY-MM-DD'),'201000017','Can Tho','0901000017','Dieu phoi vien','Than kinh','TP HCM','DPV_017');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_018','Do Thi Huong','Nu',TO_DATE('1992-04-22','YYYY-MM-DD'),'201000018','Vung Tau','0901000018','Dieu phoi vien','Tim mach','TP HCM','DPV_018');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_019','Nguyen Thi Phuong','Nu',TO_DATE('1984-08-15','YYYY-MM-DD'),'201000019','TP HCM','0901000019','Dieu phoi vien','Tieu hoa','TP HCM','DPV_019');
    INSERT INTO NHANVIEN VALUES ('NV_DPV_020','Tran Van Binh','Nam',TO_DATE('1986-06-06','YYYY-MM-DD'),'201000020','Ha Noi','0901000020','Dieu phoi vien','Than kinh','Ha Noi','DPV_020');

    -- 10 Bác sĩ/Y sĩ (mẫu - thực tế có 100)
    INSERT INTO NHANVIEN VALUES ('NV_BS_001','BS Nguyen Van An','Nam',TO_DATE('1975-01-20','YYYY-MM-DD'),'202000001','TP HCM','0911000001','Bac si/Y si','Tieu hoa','TP HCM','BS_001');
    INSERT INTO NHANVIEN VALUES ('NV_BS_002','BS Tran Thi Cam','Nu',TO_DATE('1978-05-14','YYYY-MM-DD'),'202000002','Ha Noi','0911000002','Bac si/Y si','Than kinh','Ha Noi','BS_002');
    INSERT INTO NHANVIEN VALUES ('NV_BS_003','BS Le Van Cuong','Nam',TO_DATE('1980-09-30','YYYY-MM-DD'),'202000003','Da Nang','0911000003','Bac si/Y si','Tim mach','TP HCM','BS_003');
    INSERT INTO NHANVIEN VALUES ('NV_BS_004','BS Pham Thi Dao','Nu',TO_DATE('1976-04-25','YYYY-MM-DD'),'202000004','TP HCM','0911000004','Bac si/Y si','Tieu hoa','TP HCM','BS_004');
    INSERT INTO NHANVIEN VALUES ('NV_BS_005','BS Hoang Van Em','Nam',TO_DATE('1982-12-08','YYYY-MM-DD'),'202000005','Hai Phong','0911000005','Bac si/Y si','Than kinh','Hai Phong','BS_005');
    INSERT INTO NHANVIEN VALUES ('NV_BS_006','BS Vu Thi Phuong','Nu',TO_DATE('1979-07-17','YYYY-MM-DD'),'202000006','TP HCM','0911000006','Bac si/Y si','Tim mach','TP HCM','BS_006');
    INSERT INTO NHANVIEN VALUES ('NV_BS_007','BS Dang Van Giang','Nam',TO_DATE('1977-02-28','YYYY-MM-DD'),'202000007','Ha Noi','0911000007','Bac si/Y si','Tieu hoa','Ha Noi','BS_007');
    INSERT INTO NHANVIEN VALUES ('NV_BS_008','BS Bui Thi Hang','Nu',TO_DATE('1981-10-05','YYYY-MM-DD'),'202000008','TP HCM','0911000008','Bac si/Y si','Than kinh','TP HCM','BS_008');
    INSERT INTO NHANVIEN VALUES ('NV_BS_009','BS Do Van Hy','Nam',TO_DATE('1974-06-11','YYYY-MM-DD'),'202000009','Hue','0911000009','Bac si/Y si','Tim mach','Hai Phong','BS_009');
    INSERT INTO NHANVIEN VALUES ('NV_BS_010','BS Nguyen Thi Kim','Nu',TO_DATE('1983-03-03','YYYY-MM-DD'),'202000010','TP HCM','0911000010','Bac si/Y si','Tieu hoa','TP HCM','BS_010');

    -- 5 Kỹ thuật viên (mẫu - thực tế có 50)
    INSERT INTO NHANVIEN VALUES ('NV_KTV_001','KTV Nguyen Van Long','Nam',TO_DATE('1990-04-01','YYYY-MM-DD'),'203000001','TP HCM','0921000001','Ky thuat vien','Xet nghiem','TP HCM','KTV_001');
    INSERT INTO NHANVIEN VALUES ('NV_KTV_002','KTV Tran Thi My','Nu',TO_DATE('1992-08-22','YYYY-MM-DD'),'203000002','Ha Noi','0921000002','Ky thuat vien','Chan doan hinh anh','Ha Noi','KTV_002');
    INSERT INTO NHANVIEN VALUES ('NV_KTV_003','KTV Le Van Nam','Nam',TO_DATE('1991-01-17','YYYY-MM-DD'),'203000003','Da Nang','0921000003','Ky thuat vien','Xet nghiem','TP HCM','KTV_003');
    INSERT INTO NHANVIEN VALUES ('NV_KTV_004','KTV Pham Thi Oanh','Nu',TO_DATE('1993-05-30','YYYY-MM-DD'),'203000004','TP HCM','0921000004','Ky thuat vien','Chan doan hinh anh','TP HCM','KTV_004');
    INSERT INTO NHANVIEN VALUES ('NV_KTV_005','KTV Hoang Van Phuc','Nam',TO_DATE('1989-11-09','YYYY-MM-DD'),'203000005','Hai Phong','0921000005','Ky thuat vien','Xet nghiem','Hai Phong','KTV_005');

    COMMIT;

    -- 4.2 BỆNH NHÂN (10 mẫu - thực tế có 100,000)
    INSERT INTO BENHNHAN VALUES ('BN_001','Nguyen Thi Anh','Nu',TO_DATE('1980-01-01','YYYY-MM-DD'),'310000001','12','Le Loi','Q1','TP HCM','Cao huyet ap','Tieu duong type 2',NULL,'BN_001');
    INSERT INTO BENHNHAN VALUES ('BN_002','Tran Van Binh','Nam',TO_DATE('1975-06-15','YYYY-MM-DD'),'310000002','45','Nguyen Hue','Q3','TP HCM',NULL,NULL,NULL,'BN_002');
    INSERT INTO BENHNHAN VALUES ('BN_003','Le Thi Cam','Nu',TO_DATE('1990-09-20','YYYY-MM-DD'),'310000003','78','Pasteur','Q5','TP HCM','Hen suyen',NULL,'Penicillin','BN_003');
    INSERT INTO BENHNHAN VALUES ('BN_004','Pham Van Dung','Nam',TO_DATE('1968-12-05','YYYY-MM-DD'),'310000004','23','Ba Trieu','Hoan Kiem','Ha Noi','Benh tim mach',NULL,NULL,'BN_004');
    INSERT INTO BENHNHAN VALUES ('BN_005','Hoang Thi Em','Nu',TO_DATE('1985-04-10','YYYY-MM-DD'),'310000005','56','Tran Phu','Hai Chau','Da Nang',NULL,'Ung thu vu','Aspirin','BN_005');
    INSERT INTO BENHNHAN VALUES ('BN_006','Vu Van Phong','Nam',TO_DATE('1972-08-25','YYYY-MM-DD'),'310000006','89','Le Duan','Thanh Khe','Da Nang','Tieu duong',NULL,NULL,'BN_006');
    INSERT INTO BENHNHAN VALUES ('BN_007','Dang Thi Giang','Nu',TO_DATE('1995-02-14','YYYY-MM-DD'),'310000007','11','Hung Vuong','Ngo Quyen','Hai Phong',NULL,NULL,NULL,'BN_007');
    INSERT INTO BENHNHAN VALUES ('BN_008','Bui Van Hai','Nam',TO_DATE('1960-07-07','YYYY-MM-DD'),'310000008','34','Tran Hung Dao','Le Chan','Hai Phong','Xuong khop','Tim mach',NULL,'BN_008');
    INSERT INTO BENHNHAN VALUES ('BN_009','Do Thi Yen','Nu',TO_DATE('1988-11-11','YYYY-MM-DD'),'310000009','67','Nguyen Trai','Thanh Xuan','Ha Noi',NULL,NULL,'Sulfa','BN_009');
    INSERT INTO BENHNHAN VALUES ('BN_010','Nguyen Van Khoa','Nam',TO_DATE('1978-03-28','YYYY-MM-DD'),'310000010','90','Vo Thi Sau','Q3','TP HCM','Gan','Tieu duong',NULL,'BN_010');

    COMMIT;

    -- 4.3 HỒ SƠ BỆNH ÁN
    INSERT INTO HSBA VALUES ('HSBA_001','BN_001',TO_DATE('2026-01-10','YYYY-MM-DD'),'Viem da day cap','Dung thuoc giam acid va nghi ngoi','NV_BS_001','Tieu hoa',NULL);
    INSERT INTO HSBA VALUES ('HSBA_002','BN_002',TO_DATE('2026-01-12','YYYY-MM-DD'),'Roi loan lo au','Lieu phap tam ly va thuoc giam lo','NV_BS_002','Than kinh',NULL);
    INSERT INTO HSBA VALUES ('HSBA_003','BN_003',TO_DATE('2026-01-15','YYYY-MM-DD'),'Roi loan nhip tim','Thuoc dieu chinh nhip tim','NV_BS_003','Tim mach',NULL);
    INSERT INTO HSBA VALUES ('HSBA_004','BN_004',TO_DATE('2026-01-20','YYYY-MM-DD'),'Viem loet ta trang','Phau thuat cat bo','NV_BS_001','Tieu hoa',NULL);
    INSERT INTO HSBA VALUES ('HSBA_005','BN_005',TO_DATE('2026-01-25','YYYY-MM-DD'),'Nghi ngo dau hieu that trai','Theo doi va sieu am tim','NV_BS_003','Tim mach',NULL);
    INSERT INTO HSBA VALUES ('HSBA_006','BN_001',TO_DATE('2026-02-01','YYYY-MM-DD'),'Cao huyet ap cap','Thuoc ha ap','NV_BS_006','Tim mach',NULL);

    COMMIT;

    -- 4.4 DỊCH VỤ HỖ TRỢ CHẨN ĐOÁN
    INSERT INTO HSBA_DV VALUES ('HSBA_001','Xet nghiem mau',TO_DATE('2026-01-10','YYYY-MM-DD'),'NV_KTV_001','Bach cau tang nhe');
    INSERT INTO HSBA_DV VALUES ('HSBA_001','Sieu am bung',TO_DATE('2026-01-10','YYYY-MM-DD'),'NV_KTV_002','Co quan binh thuong');
    INSERT INTO HSBA_DV VALUES ('HSBA_002','Dien nao do',TO_DATE('2026-01-12','YYYY-MM-DD'),'NV_KTV_003','Hoat dong nao binh thuong');
    INSERT INTO HSBA_DV VALUES ('HSBA_003','Dien tam do ECG',TO_DATE('2026-01-15','YYYY-MM-DD'),'NV_KTV_004','Nhip tim loan nhe');
    INSERT INTO HSBA_DV VALUES ('HSBA_004','Noi soi da day',TO_DATE('2026-01-20','YYYY-MM-DD'),'NV_KTV_001','Vet loet 5mm tanh trang');
    INSERT INTO HSBA_DV VALUES ('HSBA_005','Sieu am tim',TO_DATE('2026-01-25','YYYY-MM-DD'),'NV_KTV_004','Phan suat tong mau EF 55%');

    COMMIT;

    -- 4.5 ĐƠN THUỐC
    INSERT INTO DONTHUOC VALUES ('HSBA_001',TO_DATE('2026-01-10','YYYY-MM-DD'),'Omeprazole 20mg','Uong 1 vien/ngay truoc an sang 30 phut, dung truoc 7 ngay');
    INSERT INTO DONTHUOC VALUES ('HSBA_001',TO_DATE('2026-01-10','YYYY-MM-DD'),'Domperidone 10mg','Uong 3 vien/ngay truoc an 15-30 phut, lien tuc 5 ngay');
    INSERT INTO DONTHUOC VALUES ('HSBA_002',TO_DATE('2026-01-12','YYYY-MM-DD'),'Alprazolam 0.25mg','Uong 1 vien truoc khi di ngu, theo doi phan ung');
    INSERT INTO DONTHUOC VALUES ('HSBA_003',TO_DATE('2026-01-15','YYYY-MM-DD'),'Bisoprolol 5mg','Uong 1 vien sang moi ngay, dung lien tuc');
    INSERT INTO DONTHUOC VALUES ('HSBA_004',TO_DATE('2026-01-20','YYYY-MM-DD'),'Pantoprazole 40mg','Uong 1 vien/ngay truoc an sang, dung 4 tuan');
    INSERT INTO DONTHUOC VALUES ('HSBA_006',TO_DATE('2026-02-01','YYYY-MM-DD'),'Amlodipine 5mg','Uong 1 vien/ngay vao buoi sang, theo doi huyet ap');

    COMMIT;

    -- 4.6 THÔNG BÁO (dữ liệu sẽ bổ sung ở phần OLS)
    INSERT INTO THONGBAO VALUES ('TB_001',N'Cuoc hop toan bo nhan vien benh vien',SYSTIMESTAMP,N'Hoi truong A');
    INSERT INTO THONGBAO VALUES ('TB_002',N'Hop khan ban giam doc: Chien luoc 2026',SYSTIMESTAMP,N'Phong hop BGD - Co so HCM');
    INSERT INTO THONGBAO VALUES ('TB_003',N'Hop lanh dao cac khoa',SYSTIMESTAMP,N'Phong hop 2');
    INSERT INTO THONGBAO VALUES ('TB_004',N'Hop lanh dao Khoa tieu hoa',SYSTIMESTAMP,N'Phong khoa Tieu hoa');
    INSERT INTO THONGBAO VALUES ('TB_005',N'Cuoc hop nhan vien Khoa tieu hoa tai HCM',SYSTIMESTAMP,N'Phong khoa Tieu hoa - CS HCM');
    INSERT INTO THONGBAO VALUES ('TB_006',N'Cuoc hop nhan vien Khoa tieu hoa tai Ha Noi',SYSTIMESTAMP,N'Phong khoa Tieu hoa - CS Ha Noi');
    INSERT INTO THONGBAO VALUES ('TB_007',N'Hop lanh dao Khoa tieu hoa va Khoa than kinh tai Hai Phong',SYSTIMESTAMP,N'Phong hop - CS Hai Phong');

    COMMIT;

    PROMPT === SCHEMA AND DATA DONE ===
