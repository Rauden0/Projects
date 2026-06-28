using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsRemoved = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsRemoved = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Publishers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsRemoved = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publishers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRemoved = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PublisherId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Price = table.Column<float>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ImagePath = table.Column<string>(type: "TEXT", nullable: true),
                    IsRemoved = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Books_Publishers_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "Publishers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Information = table.Column<string>(type: "TEXT", nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    OrderState = table.Column<int>(type: "INTEGER", nullable: false),
                    Paid = table.Column<bool>(type: "INTEGER", nullable: false),
                    TotalPrice = table.Column<float>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRemoved = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Wishlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRemoved = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wishlists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BookAuthor",
                columns: table => new
                {
                    AuthorsId = table.Column<int>(type: "INTEGER", nullable: false),
                    BooksId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookAuthor", x => new { x.AuthorsId, x.BooksId });
                    table.ForeignKey(
                        name: "FK_BookAuthor_Authors_AuthorsId",
                        column: x => x.AuthorsId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookAuthor_Books_BooksId",
                        column: x => x.BooksId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookGenre",
                columns: table => new
                {
                    BooksId = table.Column<int>(type: "INTEGER", nullable: false),
                    GenresId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookGenre", x => new { x.BooksId, x.GenresId });
                    table.ForeignKey(
                        name: "FK_BookGenre_Books_BooksId",
                        column: x => x.BooksId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BookGenre_Genres_GenresId",
                        column: x => x.GenresId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRemoved = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Price = table.Column<float>(type: "REAL", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRemoved = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WishlistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WishlistId = table.Column<int>(type: "INTEGER", nullable: false),
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRemoved = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishlistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Wishlists_WishlistId",
                        column: x => x.WishlistId,
                        principalTable: "Wishlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "Id", "IsRemoved", "Name" },
                values: new object[,]
                {
                    { 1, false, "Myron Michálek" },
                    { 2, false, "Hostimil Nguyen vanová" },
                    { 3, false, "Stojmír Brabcová" },
                    { 4, false, "Timon Švehlová" },
                    { 5, false, "Justina Průcha" },
                    { 6, false, "Rostislava Motyčková" },
                    { 7, false, "Viktorie Dolejšová" },
                    { 8, false, "Luděk Křížek" },
                    { 9, false, "Hjalmar Petrů" },
                    { 10, false, "Mojžíš Žiga" },
                    { 11, false, "Čestmíra Líbal" },
                    { 12, false, "Grant Polanský" },
                    { 13, false, "Dimitrij Kuncová" },
                    { 14, false, "Rudolfína Janatová" },
                    { 15, false, "Miron Koubová" },
                    { 16, false, "Rudolfína Miko" },
                    { 17, false, "Bernard Starý" },
                    { 18, false, "Glorie Bažant" },
                    { 19, false, "Jonáš Klečková" },
                    { 20, false, "Blanka Kvasnička" },
                    { 21, false, "Agáta Janečková" },
                    { 22, false, "Zoe Horvátová" },
                    { 23, false, "Želislava Štěrbová" },
                    { 24, false, "Ráchel Vašek" },
                    { 25, false, "Leodegar Drábek" }
                });

            migrationBuilder.InsertData(
                table: "Genres",
                columns: new[] { "Id", "IsRemoved", "Name" },
                values: new object[,]
                {
                    { 1, false, "non" },
                    { 2, false, "sed" },
                    { 3, false, "ipsam" },
                    { 4, false, "non" },
                    { 5, false, "voluptas" },
                    { 6, false, "sapiente" },
                    { 7, false, "a" },
                    { 8, false, "adipisci" },
                    { 9, false, "iure" },
                    { 10, false, "exercitationem" },
                    { 11, false, "ullam" },
                    { 12, false, "veniam" }
                });

            migrationBuilder.InsertData(
                table: "Publishers",
                columns: new[] { "Id", "IsRemoved", "Name" },
                values: new object[,]
                {
                    { 1, false, "Svatoš, Uher and Trávníčková" },
                    { 2, false, "Tomáš v.o.s." },
                    { 3, false, "Kunešová a.s." },
                    { 4, false, "Švejdová v.o.s." },
                    { 5, false, "Malá - Rýdl" },
                    { 6, false, "Jechová - Veverka" },
                    { 7, false, "Nagy v.o.s." },
                    { 8, false, "Kotek v.o.s." },
                    { 9, false, "Jašková, Trávníček and Kameníková" },
                    { 10, false, "Hanousek a.s." },
                    { 11, false, "Kaplanová - Martincová" },
                    { 12, false, "Horník, Doubravová and Kaiser" },
                    { 13, false, "Červinková - Smola" },
                    { 14, false, "Matouš - Berky" },
                    { 15, false, "Chvojková - Kačírek" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "DisplayName", "Email", "IsRemoved", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { 1, "Admin", "admin@bookhub.cz", false, "HASHED_admin", 1 },
                    { 2, "Arnost17", "Mnata54@volny.cz", false, "HASHED_Kornel.Vaculikova", 0 },
                    { 3, "Paskal_Horvathova77", "Kajetan_Kubik56@atlas.cz", false, "HASHED_Aneta51", 0 },
                    { 4, "Ralf_Hulka", "Romuald.Jakoubek12@volny.cz", false, "HASHED_Griselda.Dudek61", 0 },
                    { 5, "Ivana_Pecha", "Blahomil40@centrum.cz", false, "HASHED_Evald89", 0 },
                    { 6, "Diviska.Kacirkova", "Zacharias.Vaskova11@centrum.cz", false, "HASHED_Davida76", 0 },
                    { 7, "Jarolim98", "Kvetoslava_Hoskova@centrum.cz", false, "HASHED_Nathanael.Kunc", 0 },
                    { 8, "Honorius.Zimova", "Dimitrij.Kovarova@gmail.com", false, "HASHED_Iva.Skrivanek69", 0 },
                    { 9, "Gedeon.Sestak43", "Antal_Gabrielova@centrum.cz", false, "HASHED_Horymir25", 0 },
                    { 10, "Milovin26", "Magnus60@gmail.com", false, "HASHED_Pravoslava.Kasparkova", 0 },
                    { 11, "Terezie_Zak", "Rosamunda.Vaclavikova23@atlas.cz", false, "HASHED_Magdalena.Nejedly1", 0 },
                    { 12, "Raimund_Zemanek22", "Sebastian.Ondrackova@atlas.cz", false, "HASHED_Horymir.Peterka", 0 },
                    { 13, "Drahos.Konecny", "Jolana36@atlas.cz", false, "HASHED_Vidor.Albrecht", 0 },
                    { 14, "Miloslava94", "Rebeka7@centrum.cz", false, "HASHED_Dalibor.Vymazal86", 0 },
                    { 15, "Ivona_Andrle", "Bozislav.Jasek@seznam.cz", false, "HASHED_Vlastislav59", 0 },
                    { 16, "Zikmund_Vymazal", "Gvendolina.Subrtova44@centrum.cz", false, "HASHED_Velislava_Plachy", 0 },
                    { 17, "Moric_Stehlik35", "Greta_Jelinek@gmail.com", false, "HASHED_Dina28", 0 },
                    { 18, "Estela_Peroutka49", "Adolf_Dostalova@gmail.com", false, "HASHED_Robinson.Tomek", 0 },
                    { 19, "Zlatomir.Tancos21", "Solveig.Fialova@gmail.com", false, "HASHED_Zinaida_Mrkvickova", 0 },
                    { 20, "Vojen98", "Bohuslav23@atlas.cz", false, "HASHED_Damiana60", 0 },
                    { 21, "Kveton75", "Jakub_Volna@volny.cz", false, "HASHED_Kolombina_Smrcka17", 0 },
                    { 22, "Iljana_Kadlec", "Ranek_Volek@gmail.com", false, "HASHED_Luboslava.Navratilova57", 0 },
                    { 23, "Lada_Kosova", "Renata12@gmail.com", false, "HASHED_Lubomira66", 0 },
                    { 24, "Sinkler.Stara55", "Simeona_Kubesova@centrum.cz", false, "HASHED_Budivoj69", 0 },
                    { 25, "Pravoslava37", "Aldo_Smerda@atlas.cz", false, "HASHED_Pribyslav.Hanackova79", 0 },
                    { 26, "Rajsa50", "Gracie.Rezkova90@volny.cz", false, "HASHED_Damiana81", 0 },
                    { 27, "Jindra50", "Natalie.Chaloupka3@volny.cz", false, "HASHED_Filibert_Kasparova31", 0 },
                    { 28, "Slavek.Mikova", "Zenon_Nesvadba29@atlas.cz", false, "HASHED_Zlatomira.Najmanova28", 0 },
                    { 29, "Ludomir_Hlouskova", "Sinkler.Houdkova46@gmail.com", false, "HASHED_Odeta_Manak73", 0 },
                    { 30, "Alfons.Sandova", "Damaris.Mlejnek@seznam.cz", false, "HASHED_Bretislava10", 0 }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "CreatedAt", "Description", "ImagePath", "IsRemoved", "Name", "Price", "PublisherId" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Minima quasi ipsam doloribus quis odit exercitationem placeat et neque voluptas iste.", null, false, "Quam eum ullam.", 345.6067f, 9 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Nemo dolores dolorem dolorem minus asperiores et atque dolorem praesentium dolores optio.", null, false, "Est expedita optio.", 202.88036f, 11 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ut fugit cumque error neque eius aut veniam porro omnis omnis totam.", null, false, "Quia et beatae.", 238.0615f, 10 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quibusdam quis distinctio dolores unde architecto est molestias praesentium facilis quibusdam illo.", null, false, "Sunt pariatur quidem.", 417.03665f, 7 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Amet dolores tenetur dolores ratione repudiandae nesciunt aut sit odio accusamus perferendis.", null, false, "Porro et in.", 309.2449f, 13 },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Recusandae quo et rerum et eaque aut illo cumque in numquam incidunt.", null, false, "Molestiae pariatur ut.", 398.1443f, 2 },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Et beatae saepe et est quis quas magnam provident quidem qui quos.", null, false, "Quae illum porro.", 429.07956f, 13 },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Et debitis velit et eum blanditiis rerum beatae ducimus laboriosam earum et.", null, false, "Officiis quia ut.", 206.69057f, 5 },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Velit nulla tenetur ipsa veniam totam id eligendi enim ea amet error.", null, false, "Qui velit vel.", 380.80524f, 12 },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dolorum ipsum voluptate sunt quae dolorum hic quam sed doloremque error mollitia.", null, false, "Nisi voluptatem deleniti.", 252.60945f, 5 },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reprehenderit accusantium magnam nemo numquam voluptate cum recusandae fugit illo possimus excepturi.", null, false, "Repellat nostrum sed.", 353.52042f, 9 },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Optio et autem est commodi modi consequatur dolorum vitae iusto ipsum aspernatur.", null, false, "Animi et impedit.", 428.59583f, 2 },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Vel ipsam est ipsum voluptate omnis minus sequi recusandae error consectetur et.", null, false, "Amet ad et.", 307.03854f, 7 },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Et ut et qui doloremque et et nobis magni aut quas quo.", null, false, "Sapiente delectus harum.", 205.53908f, 15 },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Amet veniam vero ut nostrum saepe veritatis similique molestiae expedita vel quo.", null, false, "Similique labore accusantium.", 317.64792f, 15 },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rem voluptas est aliquam aliquid excepturi non atque ad temporibus tempora ipsam.", null, false, "Quia magni exercitationem.", 374.75998f, 9 },
                    { 17, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Minima soluta odit explicabo voluptas qui iusto dolorem id qui molestiae aut.", null, false, "Fuga libero dolores.", 321.86362f, 1 },
                    { 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dolores est laudantium est fugiat assumenda sit illo perferendis odit fugiat aliquam.", null, false, "Exercitationem impedit necessitatibus.", 353.48187f, 1 },
                    { 19, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Eligendi non voluptatem sed qui reprehenderit laboriosam et placeat quis aspernatur excepturi.", null, false, "Consectetur quae veritatis.", 360.99533f, 12 },
                    { 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Aut eveniet sed est dignissimos voluptates omnis eaque enim ut animi similique.", null, false, "Recusandae est eos.", 427.28583f, 2 },
                    { 21, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Enim voluptas nobis nobis ex suscipit consequuntur laboriosam dicta in ut ut.", null, false, "Labore molestiae excepturi.", 243.30276f, 5 },
                    { 22, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "A nostrum nulla laboriosam dolore dolorum dolore dolorem fugiat ipsa id eos.", null, false, "Ut est illum.", 397.20663f, 5 },
                    { 23, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Voluptatum dolorem et dolor maxime error architecto eum ex molestias perspiciatis et.", null, false, "Vero qui adipisci.", 150.34575f, 1 },
                    { 24, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Consequuntur hic vitae ipsum veritatis ratione odio occaecati cupiditate nostrum reprehenderit deleniti.", null, false, "Cumque et odit.", 228.833f, 2 },
                    { 25, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Voluptas id est rerum non est est tenetur necessitatibus aut nihil nam.", null, false, "Autem tenetur sed.", 399.09235f, 1 },
                    { 26, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Neque voluptates quas voluptatem esse sit atque expedita modi enim soluta et.", null, false, "Aperiam earum porro.", 178.2647f, 6 },
                    { 27, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Amet sapiente adipisci totam repudiandae modi enim eius ipsum eaque minus at.", null, false, "Dolorem perspiciatis aut.", 357.67044f, 5 },
                    { 28, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tenetur laudantium ducimus aspernatur nihil quisquam et voluptatibus doloribus blanditiis sunt ab.", null, false, "Ex ut perspiciatis.", 389.13806f, 11 },
                    { 29, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dolorem architecto maxime magnam dolore est adipisci et dolore voluptatibus placeat aut.", null, false, "Sed quia rerum.", 309.59082f, 15 },
                    { 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Odio cumque explicabo repellendus voluptas et hic atque molestiae voluptas eius ipsum.", null, false, "Voluptas consequatur unde.", 261.74844f, 5 },
                    { 31, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Blanditiis veniam autem exercitationem nihil omnis aliquam exercitationem dolor id dolores quia.", null, false, "Velit minus maxime.", 286.865f, 7 },
                    { 32, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ad fugiat accusantium nesciunt veritatis eum quibusdam voluptas quia et aut harum.", null, false, "Voluptas nobis dolore.", 412.28302f, 10 },
                    { 33, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sed ipsa sunt magnam id molestiae voluptas quos qui molestias blanditiis nemo.", null, false, "Omnis architecto repellendus.", 290.40756f, 5 },
                    { 34, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ipsa commodi nihil qui qui blanditiis repellat provident vel at minima distinctio.", null, false, "Blanditiis atque quia.", 241.10587f, 2 },
                    { 35, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Non quisquam aut repellat ut voluptates amet libero voluptatum quia laboriosam blanditiis.", null, false, "Earum aut libero.", 406.94797f, 10 },
                    { 36, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Et consectetur quis sed qui officia in quisquam ex quasi natus fugiat.", null, false, "Occaecati voluptatem placeat.", 234.5004f, 7 },
                    { 37, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Autem minus autem consequuntur quas qui sint sint sint magni expedita beatae.", null, false, "Sint quia quibusdam.", 307.02307f, 4 },
                    { 38, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Blanditiis facere minus totam repellendus dolores provident ad sit quo fuga fugiat.", null, false, "Possimus excepturi omnis.", 208.36693f, 14 },
                    { 39, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Et temporibus est fuga fugit id at quis tempora blanditiis sint rerum.", null, false, "Dolorem dolore inventore.", 400.0073f, 13 },
                    { 40, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Aut voluptas voluptatum sit officia mollitia voluptas pariatur quia et necessitatibus vitae.", null, false, "Aut quia qui.", 366.8537f, 4 }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "CreatedAt", "Information", "IsRemoved", "OrderState", "Paid", "PaymentMethod", "TotalPrice", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quaerat quo voluptatem odio vel.", false, 1, false, 1, 309.2449f, 19 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Voluptate non ex perspiciatis asperiores.", false, 3, true, 3, 416.73386f, 22 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Non id nostrum labore in.", false, 3, true, 3, 2530.9922f, 17 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Et ea vero sit sequi.", false, 1, false, 3, 614.0771f, 8 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sed ipsum natus tempore sed.", false, 3, true, 1, 860.595f, 29 },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sed autem voluptatem et et.", false, 2, true, 3, 241.10587f, 21 },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Aut blanditiis voluptas voluptatem ipsam.", false, 1, false, 1, 469.0008f, 25 },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Nesciunt omnis eos aliquam ad.", false, 2, true, 1, 2288.4412f, 2 },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Esse non earum minima quia.", false, 4, true, 3, 1989.4977f, 23 },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Voluptate ratione quia cupiditate accusamus.", false, 4, true, 1, 971.9717f, 2 },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ratione quo dolorem fugiat odit.", false, 1, false, 3, 2012.749f, 26 },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Provident nobis et id nulla.", false, 2, true, 3, 1466.7849f, 25 },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ullam sed est omnis ea.", false, 2, false, 1, 1498.6584f, 2 },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Repudiandae repellat molestias ex exercitationem.", false, 4, true, 3, 614.04614f, 1 },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Odio similique aperiam possimus est.", false, 4, true, 1, 834.0733f, 22 },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Occaecati sequi rerum libero corporis.", false, 3, true, 2, 2013.6384f, 13 },
                    { 17, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tempore nihil inventore omnis fugiat.", false, 2, false, 3, 321.86362f, 8 },
                    { 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Et dolores fuga ut tenetur.", false, 4, true, 1, 1554.3079f, 7 },
                    { 19, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ea aut voluptatum rerum et.", false, 3, true, 2, 1220.8439f, 8 },
                    { 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Alias et laborum minus vel.", false, 2, false, 3, 2840.7324f, 3 }
                });

            migrationBuilder.InsertData(
                table: "Wishlists",
                columns: new[] { "Id", "CreatedAt", "IsRemoved", "Name", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Hic voluptatem est.", 29 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Totam voluptas libero.", 5 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Culpa porro animi.", 2 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Quis quia quo.", 20 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Est sequi eius.", 29 },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Ratione ut omnis.", 5 },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Eius voluptas inventore.", 28 },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Hic placeat nisi.", 19 },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Dolore optio dicta.", 18 },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Beatae et et.", 27 },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Dolor magni possimus.", 17 },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Ad nisi voluptas.", 28 },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Debitis perspiciatis voluptatem.", 14 },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Et et temporibus.", 7 },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Voluptatem voluptas itaque.", 19 },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Nihil quae sunt.", 5 },
                    { 17, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Autem doloremque qui.", 25 },
                    { 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Sed quo rerum.", 21 },
                    { 19, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Non soluta aut.", 30 },
                    { 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Officia vero et.", 1 },
                    { 21, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Corrupti est placeat.", 27 },
                    { 22, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Animi consequuntur quasi.", 13 },
                    { 23, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Voluptates atque laborum.", 14 },
                    { 24, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Cum corrupti fugiat.", 10 },
                    { 25, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Eaque molestiae non.", 4 },
                    { 26, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Hic dolorem sit.", 8 },
                    { 27, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Expedita nesciunt est.", 2 },
                    { 28, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Eum est perferendis.", 25 },
                    { 29, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Aut et et.", 13 },
                    { 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Ullam soluta incidunt.", 3 },
                    { 31, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Sapiente hic voluptas.", 8 },
                    { 32, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Tempora sunt enim.", 20 },
                    { 33, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Nulla voluptas reiciendis.", 2 },
                    { 34, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Voluptas voluptate et.", 20 },
                    { 35, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Ad impedit placeat.", 10 }
                });

            migrationBuilder.InsertData(
                table: "BookAuthor",
                columns: new[] { "AuthorsId", "BooksId" },
                values: new object[,]
                {
                    { 1, 9 },
                    { 1, 17 },
                    { 1, 32 },
                    { 1, 37 },
                    { 1, 40 },
                    { 2, 5 },
                    { 2, 19 },
                    { 2, 22 },
                    { 2, 23 },
                    { 2, 29 },
                    { 3, 1 },
                    { 3, 13 },
                    { 3, 18 },
                    { 3, 40 },
                    { 4, 20 },
                    { 4, 23 },
                    { 4, 28 },
                    { 4, 31 },
                    { 4, 38 },
                    { 5, 8 },
                    { 5, 32 },
                    { 5, 34 },
                    { 5, 35 },
                    { 6, 6 },
                    { 6, 8 },
                    { 6, 39 },
                    { 7, 12 },
                    { 7, 33 },
                    { 7, 39 },
                    { 8, 3 },
                    { 8, 7 },
                    { 8, 8 },
                    { 8, 15 },
                    { 8, 16 },
                    { 8, 37 },
                    { 9, 8 },
                    { 9, 10 },
                    { 9, 16 },
                    { 9, 19 },
                    { 9, 21 },
                    { 9, 29 },
                    { 9, 33 },
                    { 10, 9 },
                    { 10, 12 },
                    { 10, 16 },
                    { 11, 1 },
                    { 11, 6 },
                    { 11, 13 },
                    { 11, 25 },
                    { 11, 28 },
                    { 11, 38 },
                    { 12, 12 },
                    { 12, 36 },
                    { 13, 3 },
                    { 13, 11 },
                    { 13, 13 },
                    { 13, 28 },
                    { 13, 31 },
                    { 14, 3 },
                    { 14, 12 },
                    { 14, 16 },
                    { 14, 20 },
                    { 14, 39 },
                    { 15, 5 },
                    { 15, 22 },
                    { 15, 31 },
                    { 15, 34 },
                    { 15, 37 },
                    { 16, 1 },
                    { 16, 2 },
                    { 16, 14 },
                    { 16, 24 },
                    { 16, 26 },
                    { 17, 11 },
                    { 17, 25 },
                    { 17, 30 },
                    { 17, 34 },
                    { 17, 36 },
                    { 17, 38 },
                    { 18, 3 },
                    { 18, 32 },
                    { 18, 36 },
                    { 18, 38 },
                    { 19, 7 },
                    { 19, 35 },
                    { 20, 1 },
                    { 20, 2 },
                    { 20, 6 },
                    { 20, 14 },
                    { 20, 26 },
                    { 20, 27 },
                    { 20, 36 },
                    { 20, 38 },
                    { 21, 19 },
                    { 21, 22 },
                    { 22, 2 },
                    { 22, 7 },
                    { 22, 10 },
                    { 22, 24 },
                    { 22, 35 },
                    { 23, 4 },
                    { 23, 18 },
                    { 23, 39 },
                    { 24, 5 },
                    { 24, 27 },
                    { 25, 24 },
                    { 25, 30 }
                });

            migrationBuilder.InsertData(
                table: "BookGenre",
                columns: new[] { "BooksId", "GenresId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 11 },
                    { 2, 11 },
                    { 3, 1 },
                    { 3, 11 },
                    { 4, 11 },
                    { 5, 2 },
                    { 5, 6 },
                    { 5, 9 },
                    { 6, 2 },
                    { 7, 10 },
                    { 7, 11 },
                    { 8, 7 },
                    { 8, 8 },
                    { 8, 12 },
                    { 9, 1 },
                    { 9, 9 },
                    { 10, 4 },
                    { 10, 7 },
                    { 11, 11 },
                    { 12, 1 },
                    { 12, 10 },
                    { 12, 12 },
                    { 13, 3 },
                    { 13, 5 },
                    { 13, 10 },
                    { 13, 11 },
                    { 14, 9 },
                    { 14, 12 },
                    { 15, 4 },
                    { 16, 5 },
                    { 16, 9 },
                    { 16, 11 },
                    { 17, 3 },
                    { 18, 5 },
                    { 19, 5 },
                    { 19, 11 },
                    { 20, 7 },
                    { 20, 9 },
                    { 21, 3 },
                    { 22, 2 },
                    { 22, 6 },
                    { 23, 8 },
                    { 24, 2 },
                    { 24, 4 },
                    { 25, 4 },
                    { 25, 5 },
                    { 26, 1 },
                    { 26, 3 },
                    { 27, 11 },
                    { 28, 6 },
                    { 29, 1 },
                    { 30, 2 },
                    { 30, 7 },
                    { 31, 5 },
                    { 32, 1 },
                    { 32, 8 },
                    { 33, 1 },
                    { 33, 7 },
                    { 34, 5 },
                    { 34, 11 },
                    { 35, 8 },
                    { 35, 11 },
                    { 36, 5 },
                    { 37, 7 },
                    { 37, 10 },
                    { 38, 4 },
                    { 38, 7 },
                    { 38, 10 },
                    { 39, 2 },
                    { 39, 6 },
                    { 39, 7 },
                    { 39, 9 },
                    { 40, 7 }
                });

            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "Id", "AddedAt", "BookId", "IsRemoved", "OrderId", "Price", "Quantity" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, 1, 309.2449f, 1 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 38, false, 2, 208.36693f, 2 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 39, false, 3, 400.0073f, 3 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, false, 3, 206.69057f, 1 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 16, false, 3, 374.75998f, 3 },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 13, false, 4, 307.03854f, 2 },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 31, false, 5, 286.865f, 3 },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 34, false, 6, 241.10587f, 1 },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 36, false, 7, 234.5004f, 2 },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 35, false, 8, 406.94797f, 3 },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, 8, 345.6067f, 1 },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 19, false, 8, 360.99533f, 2 },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 39, false, 9, 400.0073f, 2 },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, 9, 309.2449f, 3 },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 30, false, 9, 261.74844f, 1 },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 18, false, 10, 353.48187f, 1 },
                    { 17, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, 10, 309.2449f, 2 },
                    { 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 34, false, 11, 241.10587f, 1 },
                    { 19, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 32, false, 11, 412.28302f, 3 },
                    { 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 26, false, 11, 178.2647f, 3 },
                    { 21, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 21, false, 12, 243.30276f, 3 },
                    { 22, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20, false, 12, 427.28583f, 1 },
                    { 23, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 29, false, 12, 309.59082f, 1 },
                    { 24, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 22, false, 13, 397.20663f, 3 },
                    { 25, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 13, false, 13, 307.03854f, 1 },
                    { 26, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 37, false, 14, 307.02307f, 2 },
                    { 27, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, false, 15, 417.03665f, 2 },
                    { 28, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 33, false, 16, 290.40756f, 3 },
                    { 29, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, false, 16, 380.80524f, 3 },
                    { 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 17, false, 17, 321.86362f, 1 },
                    { 31, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 38, false, 18, 208.36693f, 2 },
                    { 32, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 30, false, 18, 261.74844f, 2 },
                    { 33, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 13, false, 18, 307.03854f, 2 },
                    { 34, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 35, false, 19, 406.94797f, 3 },
                    { 35, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 14, false, 20, 205.53908f, 2 },
                    { 36, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, false, 20, 429.07956f, 3 },
                    { 37, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, false, 20, 380.80524f, 3 }
                });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "Id", "BookId", "Comment", "CreatedAt", "IsRemoved", "Rating", "UserId" },
                values: new object[,]
                {
                    { 1, 34, "Ut id non nisi perferendis debitis sit natus soluta et.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 26 },
                    { 2, 14, "Excepturi totam repudiandae veritatis est est expedita consectetur recusandae.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 20 },
                    { 3, 32, "Sed quas numquam dicta vel consequatur nulla quo qui quibusdam.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 24 },
                    { 4, 30, "Doloremque sint tempora numquam.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 19 },
                    { 5, 25, "Aliquam voluptas est illo id suscipit omnis.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 30 },
                    { 6, 7, "Sint reiciendis inventore laudantium non qui voluptatibus sequi maiores non.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 25 },
                    { 7, 12, "In et enim nihil.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 21 },
                    { 8, 33, "Harum dolore rerum enim possimus et excepturi inventore omnis facilis.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 17 },
                    { 9, 35, "Ut qui optio qui.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 4, 3 },
                    { 10, 19, "Officia ipsa laudantium debitis nesciunt impedit quas aspernatur.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 30 },
                    { 11, 30, "Nisi vel culpa.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 18 },
                    { 12, 3, "Non voluptatem voluptatem sed blanditiis et voluptate expedita.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 7 },
                    { 13, 1, "Ipsa consequatur asperiores expedita nulla pariatur aut totam aut.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 25 },
                    { 14, 28, "Totam expedita dolorem sequi officiis.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 17 },
                    { 15, 14, "Sequi corrupti similique quo consequatur vel facere ab nobis.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 4, 15 },
                    { 16, 18, "Numquam aliquid nesciunt minus consequatur.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 3 },
                    { 17, 36, "Rem reiciendis eos doloribus voluptas qui.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 6 },
                    { 18, 38, "Sit modi et quas sunt consequatur voluptas quas porro.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 16 },
                    { 19, 35, "Et sed odio voluptatum repellat eos aut.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 23 },
                    { 20, 31, "Sit officia reiciendis sit sed ipsum unde quod natus.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 5 },
                    { 21, 30, "Nulla quasi qui non cum dolorem vitae.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 19 },
                    { 22, 36, "Accusamus in odit quas aut quam.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 16 },
                    { 23, 17, "Atque et accusantium dolorem delectus unde quia consequuntur doloremque.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 21 },
                    { 24, 29, "Et non velit qui rem deserunt eveniet ut eos modi.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 1 },
                    { 25, 6, "Quae recusandae eos.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 17 },
                    { 26, 13, "Ea similique cumque cumque quo dolorum officiis iure.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 28 },
                    { 27, 15, "Cupiditate deleniti sunt qui quo quia fugiat nisi id.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 4, 26 },
                    { 28, 12, "Eum corrupti neque consectetur saepe dicta qui sed.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 11 },
                    { 29, 11, "Aut omnis aut accusantium aut quis et dolorem.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 6 },
                    { 30, 25, "Ut ullam quo rem sed dicta.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 9 },
                    { 31, 12, "Aut magni quo.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 1 },
                    { 32, 39, "Facere laboriosam quia numquam est.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 6 },
                    { 33, 18, "Quisquam inventore velit et in.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 29 },
                    { 34, 37, "Deserunt autem molestiae culpa aspernatur quaerat quasi atque.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 4, 7 },
                    { 35, 36, "Est voluptas mollitia dolores cupiditate.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 26 },
                    { 36, 15, "Nisi enim laborum eius consequatur tenetur debitis in fuga.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 9 },
                    { 37, 10, "Consequatur saepe ea ipsum et hic qui.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 20 },
                    { 38, 4, "Consequatur aperiam qui.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 7 },
                    { 39, 7, "Non et et dolore odit enim commodi qui corporis adipisci.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 25 },
                    { 40, 10, "Exercitationem qui sapiente cumque sunt aliquid doloremque.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 28 },
                    { 41, 5, "Nihil mollitia numquam sit.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 4, 13 },
                    { 42, 3, "Quo cumque qui quod sed temporibus explicabo consequatur.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 7 },
                    { 43, 26, "Accusamus laboriosam ea impedit necessitatibus.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 16 },
                    { 44, 5, "Sint odit voluptatem sequi alias in quo.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 24 },
                    { 45, 16, "Corrupti deleniti molestias repudiandae.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 5 },
                    { 46, 17, "Repellat ut occaecati omnis dolorem aliquid vel consequatur consectetur quas.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 22 },
                    { 47, 19, "Modi quibusdam deserunt ut officia.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 14 },
                    { 48, 16, "Aut error vel consequatur iure fugiat quos alias.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 4 },
                    { 49, 38, "Maiores in sit.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 15 },
                    { 50, 33, "Cupiditate facilis a.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 4, 5 }
                });

            migrationBuilder.InsertData(
                table: "WishlistItems",
                columns: new[] { "Id", "AddedAt", "BookId", "IsRemoved", "WishlistId" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, 1 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 13, false, 1 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 34, false, 2 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 39, false, 2 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, false, 2 },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 16, false, 2 },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 12, false, 2 },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 12, false, 3 },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 31, false, 3 },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 34, false, 4 },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 36, false, 5 },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 35, false, 6 },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, 6 },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 19, false, 6 },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 39, false, 6 },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, 6 },
                    { 17, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 19, false, 7 },
                    { 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 18, false, 7 },
                    { 19, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, 7 },
                    { 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 34, false, 7 },
                    { 21, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 32, false, 8 },
                    { 22, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 26, false, 8 },
                    { 23, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 38, false, 8 },
                    { 24, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 21, false, 8 },
                    { 25, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20, false, 8 },
                    { 26, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 22, false, 9 },
                    { 27, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 13, false, 9 },
                    { 28, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, 9 },
                    { 29, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 37, false, 9 },
                    { 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, false, 10 },
                    { 31, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 33, false, 11 },
                    { 32, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, false, 11 },
                    { 33, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 17, false, 11 },
                    { 34, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 38, false, 12 },
                    { 35, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 30, false, 12 },
                    { 36, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 13, false, 12 },
                    { 37, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 12, false, 12 },
                    { 38, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 37, false, 13 },
                    { 39, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 14, false, 13 },
                    { 40, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, false, 13 },
                    { 41, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, false, 13 },
                    { 42, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, false, 13 },
                    { 43, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 21, false, 14 },
                    { 44, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 35, false, 14 },
                    { 45, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 22, false, 15 },
                    { 46, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 21, false, 15 },
                    { 47, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 14, false, 16 },
                    { 48, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10, false, 16 },
                    { 49, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 23, false, 16 },
                    { 50, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, false, 16 },
                    { 51, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 14, false, 17 },
                    { 52, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 26, false, 17 },
                    { 53, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 31, false, 17 },
                    { 54, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 12, false, 18 },
                    { 55, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 16, false, 19 },
                    { 56, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 12, false, 19 },
                    { 57, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 21, false, 19 },
                    { 58, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10, false, 19 },
                    { 59, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 14, false, 20 },
                    { 60, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, 21 },
                    { 61, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 19, false, 21 },
                    { 62, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 14, false, 21 },
                    { 63, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, false, 21 },
                    { 64, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 25, false, 21 },
                    { 65, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 22, false, 22 },
                    { 66, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 14, false, 23 },
                    { 67, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 38, false, 23 },
                    { 68, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, false, 24 },
                    { 69, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 33, false, 24 },
                    { 70, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 22, false, 24 },
                    { 71, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, false, 25 },
                    { 72, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 39, false, 26 },
                    { 73, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 34, false, 26 },
                    { 74, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 25, false, 26 },
                    { 75, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 19, false, 26 },
                    { 76, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 17, false, 26 },
                    { 77, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 22, false, 27 },
                    { 78, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 32, false, 27 },
                    { 79, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 25, false, 27 },
                    { 80, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 23, false, 27 },
                    { 81, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 37, false, 28 },
                    { 82, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 27, false, 28 },
                    { 83, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, false, 28 },
                    { 84, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20, false, 28 },
                    { 85, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 18, false, 29 },
                    { 86, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, false, 29 },
                    { 87, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 14, false, 29 },
                    { 88, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 26, false, 30 },
                    { 89, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 17, false, 30 },
                    { 90, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, false, 31 },
                    { 91, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 40, false, 31 },
                    { 92, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 29, false, 31 },
                    { 93, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, false, 32 },
                    { 94, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10, false, 33 },
                    { 95, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 13, false, 33 },
                    { 96, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, false, 33 },
                    { 97, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 12, false, 34 },
                    { 98, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, false, 34 },
                    { 99, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 29, false, 34 },
                    { 100, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 19, false, 34 },
                    { 101, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 32, false, 35 },
                    { 102, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 12, false, 35 },
                    { 103, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, 35 },
                    { 104, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 24, false, 35 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Authors_Name",
                table: "Authors",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BookAuthor_BooksId",
                table: "BookAuthor",
                column: "BooksId");

            migrationBuilder.CreateIndex(
                name: "IX_BookGenre_GenresId",
                table: "BookGenre",
                column: "GenresId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Name",
                table: "Books",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Books_PublisherId",
                table: "Books",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_Genres_Name",
                table: "Genres",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_BookId",
                table: "OrderItems",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Publishers_Name",
                table: "Publishers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookId",
                table: "Reviews",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_BookId",
                table: "WishlistItems",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_WishlistId",
                table: "WishlistItems",
                column: "WishlistId");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_UserId",
                table: "Wishlists",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookAuthor");

            migrationBuilder.DropTable(
                name: "BookGenre");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "WishlistItems");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Wishlists");

            migrationBuilder.DropTable(
                name: "Publishers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
