CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Courses" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Courses" PRIMARY KEY,
    "Slug" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "ShortName" TEXT NOT NULL,
    "NormalizedName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "About" TEXT NOT NULL,
    "Category" TEXT NOT NULL,
    "Modality" TEXT NOT NULL,
    "Hours" INTEGER NOT NULL,
    "Audience" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "Image" TEXT NOT NULL,
    "ImageAlt" TEXT NOT NULL,
    "AvailableAt" TEXT NULL,
    "Featured" INTEGER NOT NULL,
    "Certification" TEXT NOT NULL,
    "OnlineInformationJson" TEXT NOT NULL,
    CONSTRAINT "CK_Courses_Hours" CHECK (Hours > 0)
);

CREATE TABLE "CourseFaqs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CourseFaqs" PRIMARY KEY AUTOINCREMENT,
    "CourseId" INTEGER NOT NULL,
    "Position" INTEGER NOT NULL,
    "Question" TEXT NOT NULL,
    "Answer" TEXT NOT NULL,
    CONSTRAINT "FK_CourseFaqs_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Modules" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Modules" PRIMARY KEY AUTOINCREMENT,
    "CourseId" INTEGER NOT NULL,
    "Position" INTEGER NOT NULL,
    "Title" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    CONSTRAINT "AK_Modules_Id_CourseId" UNIQUE ("Id", "CourseId"),
    CONSTRAINT "FK_Modules_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Lessons" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Lessons" PRIMARY KEY AUTOINCREMENT,
    "CourseId" INTEGER NOT NULL,
    "ModuleId" INTEGER NULL,
    "Key" TEXT NOT NULL,
    "Position" INTEGER NOT NULL,
    "Title" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "ContentJson" TEXT NOT NULL,
    CONSTRAINT "FK_Lessons_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Lessons_Modules_ModuleId_CourseId" FOREIGN KEY ("ModuleId", "CourseId") REFERENCES "Modules" ("Id", "CourseId")
);

CREATE TABLE "ModuleTopics" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ModuleTopics" PRIMARY KEY AUTOINCREMENT,
    "ModuleId" INTEGER NOT NULL,
    "Position" INTEGER NOT NULL,
    "Text" TEXT NOT NULL,
    CONSTRAINT "FK_ModuleTopics_Modules_ModuleId" FOREIGN KEY ("ModuleId") REFERENCES "Modules" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_CourseFaqs_CourseId" ON "CourseFaqs" ("CourseId");

CREATE INDEX "IX_Courses_Category_Modality_Status" ON "Courses" ("Category", "Modality", "Status");

CREATE UNIQUE INDEX "IX_Courses_Slug" ON "Courses" ("Slug");

CREATE UNIQUE INDEX "IX_Lessons_CourseId_Key" ON "Lessons" ("CourseId", "Key");

CREATE INDEX "IX_Lessons_ModuleId_CourseId" ON "Lessons" ("ModuleId", "CourseId");

CREATE UNIQUE INDEX "IX_Modules_CourseId_Position" ON "Modules" ("CourseId", "Position");

CREATE INDEX "IX_ModuleTopics_ModuleId" ON "ModuleTopics" ("ModuleId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261023215623_InitialCatalog', '10.0.13');

COMMIT;

