CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919133058_InitialCreate') THEN
    CREATE TABLE tenants (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Slug" character varying(100) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_tenants" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919133058_InitialCreate') THEN
    CREATE TABLE workout_entries (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Date" date NOT NULL,
        "Notes" character varying(2000),
        "DurationMinutes" integer NOT NULL,
        "CaloriesBurned" integer NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_workout_entries" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_workout_entries_tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES tenants ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919133058_InitialCreate') THEN
    CREATE TABLE workout_exercises (
        "Id" uuid NOT NULL,
        "WorkoutEntryId" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Sets" integer NOT NULL,
        "Reps" integer NOT NULL,
        "WeightKg" numeric,
        "DurationMinutes" integer,
        "Notes" character varying(1000),
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_workout_exercises" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_workout_exercises_workout_entries_WorkoutEntryId" FOREIGN KEY ("WorkoutEntryId") REFERENCES workout_entries ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919133058_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_tenants_Slug" ON tenants ("Slug");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919133058_InitialCreate') THEN
    CREATE INDEX "IX_workout_entries_TenantId" ON workout_entries ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919133058_InitialCreate') THEN
    CREATE INDEX "IX_workout_exercises_WorkoutEntryId" ON workout_exercises ("WorkoutEntryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919133058_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260919133058_InitialCreate', '10.0.0');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919154733_AddApplicationUsers') THEN
    CREATE TABLE application_users (
        "Id" uuid NOT NULL,
        "Email" character varying(250) NOT NULL,
        "PasswordHash" text NOT NULL,
        "TenantId" uuid NOT NULL,
        "Role" character varying(50) NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_application_users" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_application_users_tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES tenants ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919154733_AddApplicationUsers') THEN
    CREATE UNIQUE INDEX "IX_application_users_Email" ON application_users ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919154733_AddApplicationUsers') THEN
    CREATE INDEX "IX_application_users_TenantId" ON application_users ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919154733_AddApplicationUsers') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260919154733_AddApplicationUsers', '10.0.0');
    END IF;
END $EF$;
COMMIT;

