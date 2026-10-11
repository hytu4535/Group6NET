IF OBJECT_ID(N'dbo.pet_images', N'U') IS NOT NULL
BEGIN
    ;WITH PetsWithoutAvatar AS
    (
        SELECT DISTINCT pet_id
        FROM dbo.pet_images
        WHERE pet_id IS NOT NULL
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.pet_images AS avatar
              WHERE avatar.pet_id = pet_images.pet_id
                AND ISNULL(avatar.is_avatar, 0) = 1
          )
    ),
    FirstImagePerPet AS
    (
        SELECT
            image.id,
            ROW_NUMBER() OVER (PARTITION BY image.pet_id ORDER BY image.id ASC) AS row_number
        FROM dbo.pet_images AS image
        INNER JOIN PetsWithoutAvatar AS missing
            ON missing.pet_id = image.pet_id
    )
    UPDATE image
    SET is_avatar = 1
    FROM dbo.pet_images AS image
    INNER JOIN FirstImagePerPet AS first_image
        ON first_image.id = image.id
    WHERE first_image.row_number = 1;
END;
