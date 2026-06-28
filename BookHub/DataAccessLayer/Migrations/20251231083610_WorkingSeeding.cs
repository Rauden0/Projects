using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class WorkingSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                table: "GiftCards",
                columns: new[] { "Id", "IsRemoved", "ReductionAmount", "ValidFrom", "ValidTo" },
                values: new object[,]
                {
                    { 1, false, 500m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, false, 900m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, false, 300m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, false, 300m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, false, 100m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 6, false, 400m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 7, false, 1000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 8, false, 300m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 9, false, 200m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 10, false, 300m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) }
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
                    { 19, "Zlatomir.Tancos21", "bara@mail.com", false, "HASHED_Zinaida_Mrkvickova", 0 },
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
                columns: new[] { "Id", "CreatedAt", "Description", "ImagePath", "IsRemoved", "Name", "Price", "PrimaryGenreId", "PublisherId", "StockQuantity" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quam sed doloremque error mollitia nisi ipsam repellat nostrum sed reprehenderit accusantium.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Quae dolorum hic.", 225.36723f, 11, 5, 20 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Recusandae fugit illo possimus excepturi cum similique animi et impedit optio et.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Numquam voluptate cum.", 387.9444f, 11, 11, 20 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dolorum vitae iusto ipsum aspernatur non dicta amet ad et vel ipsam.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Commodi modi consequatur.", 381.47626f, 2, 3, 20 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sequi recusandae error consectetur et cupiditate laudantium sapiente delectus harum et ut.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Voluptate omnis minus.", 299.58423f, 11, 6, 20 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Nobis magni aut quas quo amet delectus similique labore accusantium amet veniam.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Doloremque et et.", 407.65298f, 12, 9, 20 },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Similique molestiae expedita vel quo natus ut quia magni exercitationem rem voluptas.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Nostrum saepe veritatis.", 382.40402f, 7, 4, 20 },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Atque ad temporibus tempora ipsam placeat mollitia fuga libero dolores minima soluta.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Aliquid excepturi non.", 182.33621f, 1, 2, 20 },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dolorem id qui molestiae aut sunt alias exercitationem impedit necessitatibus dolores est.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Voluptas qui iusto.", 289.71722f, 3, 12, 20 },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Illo perferendis odit fugiat aliquam cum inventore consectetur quae veritatis eligendi non.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Fugiat assumenda sit.", 156.85414f, 9, 4, 20 },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Et placeat quis aspernatur excepturi nihil repellendus recusandae est eos aut eveniet.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Qui reprehenderit laboriosam.", 210.3625f, 5, 11, 20 },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Eaque enim ut animi similique molestiae vitae labore molestiae excepturi enim voluptas.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Dignissimos voluptates omnis.", 355.49396f, 7, 11, 20 },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Laboriosam dicta in ut ut enim voluptatem ut est illum a nostrum.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Ex suscipit consequuntur.", 403.5627f, 7, 6, 20 },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dolorem fugiat ipsa id eos dolorem quis vero qui adipisci voluptatum dolorem.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Dolore dolorum dolore.", 426.5418f, 2, 3, 20 },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Eum ex molestias perspiciatis et alias doloremque cumque et odit consequuntur hic.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Maxime error architecto.", 174.36795f, 4, 3, 20 },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Occaecati cupiditate nostrum reprehenderit deleniti voluptatem fugit autem tenetur sed voluptas id.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Veritatis ratione odio.", 368.0648f, 7, 10, 20 },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tenetur necessitatibus aut nihil nam eum perferendis aperiam earum porro neque voluptates.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Non est est.", 301.479f, 11, 1, 20 },
                    { 17, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Expedita modi enim soluta et explicabo qui dolorem perspiciatis aut amet sapiente.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Esse sit atque.", 208.20264f, 1, 8, 20 },
                    { 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Eius ipsum eaque minus at eligendi minima ex ut perspiciatis tenetur laudantium.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Repudiandae modi enim.", 284.8982f, 5, 2, 20 },
                    { 19, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Voluptatibus doloribus blanditiis sunt ab quibusdam porro sed quia rerum dolorem architecto.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Nihil quisquam et.", 373.91138f, 10, 4, 20 },
                    { 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Et dolore voluptatibus placeat aut provident doloribus voluptas consequatur unde odio cumque.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Dolore est adipisci.", 178.34344f, 11, 12, 20 },
                    { 21, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Atque molestiae voluptas eius ipsum quis voluptatem velit minus maxime blanditiis veniam.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Voluptas et hic.", 387.772f, 5, 5, 20 },
                    { 22, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Exercitationem dolor id dolores quia blanditiis laudantium voluptas nobis dolore ad fugiat.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Nihil omnis aliquam.", 158.22429f, 7, 3, 20 },
                    { 23, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Voluptas quia et aut harum officiis distinctio omnis architecto repellendus sed ipsa.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Veritatis eum quibusdam.", 321.96658f, 6, 4, 20 },
                    { 24, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quos qui molestias blanditiis nemo totam corporis blanditiis atque quia ipsa commodi.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Id molestiae voluptas.", 361.12244f, 1, 9, 20 },
                    { 25, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Provident vel at minima distinctio corporis architecto earum aut libero non quisquam.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Qui blanditiis repellat.", 183.07082f, 5, 15, 20 },
                    { 26, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Libero voluptatum quia laboriosam blanditiis vero dolorum occaecati voluptatem placeat et consectetur.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Ut voluptates amet.", 261.0773f, 3, 4, 20 },
                    { 27, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quisquam ex quasi natus fugiat veniam et sint quia quibusdam autem minus.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Qui officia in.", 262.54868f, 2, 2, 20 },
                    { 28, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sint sint magni expedita beatae cupiditate ut possimus excepturi omnis blanditiis facere.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Quas qui sint.", 369.67593f, 9, 8, 20 },
                    { 29, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ad sit quo fuga fugiat adipisci necessitatibus dolorem dolore inventore et temporibus.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Repellendus dolores provident.", 382.05847f, 10, 10, 20 },
                    { 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quis tempora blanditiis sint rerum fugiat quo aut quia qui aut voluptas.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Fugit id at.", 292.2989f, 9, 5, 20 },
                    { 31, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pariatur quia et necessitatibus vitae quisquam et fugiat eum laudantium doloribus ut.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Officia mollitia voluptas.", 371.26346f, 5, 14, 20 },
                    { 32, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sit natus soluta et harum ut alias qui excepturi totam repudiandae veritatis.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Nisi perferendis debitis.", 382.20273f, 4, 11, 20 },
                    { 33, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Est temporibus quia sapiente sed quas numquam dicta vel consequatur nulla quo.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Expedita consectetur recusandae.", 325.28094f, 12, 12, 20 },
                    { 34, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Voluptatem doloremque sint tempora numquam aut et quasi similique aliquam voluptas est.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Harum placeat mollitia.", 166.49644f, 4, 12, 20 },
                    { 35, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sequi omnis saepe sint reiciendis inventore laudantium non qui voluptatibus sequi maiores.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Suscipit omnis aut.", 428.66418f, 11, 11, 20 },
                    { 36, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "In et enim nihil iste illum quas saepe harum dolore rerum enim.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Veniam enim dolor.", 377.5423f, 3, 10, 20 },
                    { 37, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Facilis beatae vero soluta ipsum ut qui optio qui asperiores praesentium dicta.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Excepturi inventore omnis.", 341.96255f, 11, 9, 20 },
                    { 38, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Nesciunt impedit quas aspernatur mollitia id iure voluptatem nisi vel culpa eius.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Ipsa laudantium debitis.", 168.76695f, 1, 6, 20 },
                    { 39, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Voluptatem sed blanditiis et voluptate expedita qui aut neque dolorem ipsa consequatur.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Rerum non voluptatem.", 448.38287f, 4, 10, 20 },
                    { 40, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Totam aut omnis soluta quae nisi totam expedita dolorem sequi officiis deleniti.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Nulla pariatur aut.", 249.5732f, 9, 10, 20 }
                });

            migrationBuilder.InsertData(
                table: "Carts",
                columns: new[] { "Id", "AppliedCouponId", "CreatedAt", "IsRemoved", "UserId" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 19 },
                    { 2, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3 },
                    { 3, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 27 },
                    { 4, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 10 },
                    { 5, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5 },
                    { 6, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 15 },
                    { 7, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 6 },
                    { 8, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 20 },
                    { 9, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2 },
                    { 10, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 8 },
                    { 11, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1 },
                    { 12, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 11 },
                    { 13, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 24 },
                    { 14, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 28 }
                });

            migrationBuilder.InsertData(
                table: "Coupons",
                columns: new[] { "Id", "Code", "GiftCardId", "IsRemoved", "IsUsed", "OrderId" },
                values: new object[,]
                {
                    { 1, "GIFT-7018-5656", 7, false, false, null },
                    { 2, "GIFT-3111-7964", 5, false, false, null },
                    { 3, "GIFT-4617-1903", 8, false, true, null },
                    { 4, "GIFT-5120-2757", 3, false, false, null },
                    { 5, "GIFT-6586-7261", 1, false, false, null },
                    { 6, "GIFT-6546-7084", 7, false, false, null },
                    { 7, "GIFT-5149-1191", 1, false, false, null },
                    { 8, "GIFT-4805-8985", 9, false, false, null },
                    { 9, "GIFT-2980-0065", 3, false, false, null },
                    { 10, "GIFT-8108-7408", 7, false, false, null },
                    { 11, "GIFT-3525-6849", 9, false, false, null },
                    { 12, "GIFT-2348-4634", 1, false, false, null },
                    { 13, "GIFT-4396-1381", 5, false, false, null },
                    { 14, "GIFT-8902-4762", 2, false, false, null },
                    { 15, "GIFT-5773-0461", 1, false, false, null }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "City", "Country", "CouponId", "CreatedAt", "Email", "FirstName", "IsRemoved", "LastName", "OrderState", "Paid", "PaymentMethod", "PhoneNumber", "PostalCode", "Street", "TotalPrice", "UserId" },
                values: new object[,]
                {
                    { 1, "Vysoké Veselí", "Španělsko", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Saba_Bila@gmail.com", "Kvido", false, "Ulrich", 2, true, 2, "632 796 121", "352-60", "Tylovo Náměstí 230", 407.65298f, 19 },
                    { 2, "Chlumec", "Svatý Vincenc a Grenadiny", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rufus_Humlova@gmail.com", "Kvido", false, "Ulrich", 2, true, 2, "736 851 248", "303-61", "Ke Křížkám 917", 506.30084f, 3 },
                    { 3, "Trhové Sviny", "DR Kongo", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Aglaja_Taborska@volny.cz", "Kvido", false, "Ulrich", 3, true, 2, "736 674 510", "09529", "Nad Koupadly 36", 1642.5371f, 27 },
                    { 4, "Bohušovice nad Ohří", "Tunisko", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Skarlet.Baresova@volny.cz", "Kvido", false, "Ulrich", 1, true, 1, "601 602 622", "896-34", "U Arborky 25", 853.0836f, 10 },
                    { 5, "Třemošná", "Portugalsko", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ernest.Dostalova@centrum.cz", "Kvido", false, "Ulrich", 1, false, 3, "737 687 836", "997 30", "Županovická 68", 1113.7904f, 10 },
                    { 6, "Ostrava", "Lotyšsko", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Selena.Kacirkova@atlas.cz", "Kvido", false, "Ulrich", 2, false, 3, "601 362 106", "263-11", "Gorazdova 958", 332.9929f, 5 },
                    { 7, "Liberec", "Polsko", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Katrin_Pourova78@centrum.cz", "Kvido", false, "Ulrich", 2, false, 3, "+420 148 913 052", "208-82", "Severní Iv 1", 377.5423f, 15 },
                    { 8, "Český Brod", "Samoa", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lucie11@gmail.com", "Kvido", false, "Ulrich", 3, true, 1, "00420 434 617 092", "448-89", "Otavská 7", 2055.8857f, 6 },
                    { 9, "Přelouč", "Kostarika", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Zachar.Grundza@centrum.cz", "Kvido", false, "Ulrich", 1, false, 1, "+420 600 138 996", "215 57", "Bojčenkova 14", 2588.9685f, 20 },
                    { 10, "Holice", "Česko", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Konstancie_Svatonova11@seznam.cz", "Kvido", false, "Ulrich", 2, true, 3, "00420 310 891 109", "02699", "Pod Výšinkou 66", 1792.7554f, 6 },
                    { 11, "Uničov", "Kolumbie", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Depold39@gmail.com", "Kvido", false, "Ulrich", 3, true, 3, "737 412 546", "676 82", "Na Viničních Horách 26", 2001.7556f, 2 },
                    { 12, "Polička", "Bolívie", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Baltazar_Sladkova@seznam.cz", "Kvido", false, "Ulrich", 3, true, 3, "737 231 206", "65674", "Jetelová 487", 2462.4634f, 8 },
                    { 13, "Vítkov", "Laos", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lutibor.Kuncova96@seznam.cz", "Kvido", false, "Ulrich", 1, true, 2, "736 863 600", "28181", "Soustružnická 102", 742.99036f, 27 },
                    { 14, "Mikulášovice", "Palau", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lutobor_Michal@centrum.cz", "Kvido", false, "Ulrich", 1, true, 2, "742 957 291", "02023", "Křovinovo Nám. 4", 1025.8877f, 1 },
                    { 15, "Břidličná", "Súdán", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mirek.Hron58@centrum.cz", "Kvido", false, "Ulrich", 3, true, 3, "00420 492 346 136", "448-89", "Konopišťská 509", 898.7527f, 11 },
                    { 16, "Česká Lípa", "Libérie", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bartolomej53@gmail.com", "Kvido", false, "Ulrich", 1, true, 3, "00420 784 000 364", "602 42", "Odboje 77", 1289.5511f, 6 },
                    { 17, "Kelč", "Venezuela", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chranislava80@seznam.cz", "Kvido", false, "Ulrich", 2, true, 3, "737 243 853", "54726", "Pod Krocínkou 235", 208.20264f, 24 },
                    { 18, "Horní Benešov", "Dánsko", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Darja_Bily@centrum.cz", "Kvido", false, "Ulrich", 2, true, 2, "271 759 168", "025-52", "Jungmannovo Náměstí 6", 2325.289f, 8 },
                    { 19, "Štíty", "Tuvalu", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ireneus.Slavikova@volny.cz", "Kvido", false, "Ulrich", 2, false, 3, "736 607 970", "94013", "Lyžařská 2", 1285.9926f, 27 },
                    { 20, "Moravská Třebová", "Kuvajt", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Klaudius60@volny.cz", "Kvido", false, "Ulrich", 2, true, 2, "+420 028 291 555", "985 01", "U Jezera 4", 1358.3387f, 28 }
                });

            migrationBuilder.InsertData(
                table: "Wishlists",
                columns: new[] { "Id", "CreatedAt", "IsRemoved", "Name", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Vel modi porro.", 3 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Consectetur non harum.", 22 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Ut vel eum.", 19 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Voluptas perspiciatis quam.", 19 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Eum amet ea.", 22 },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Harum nesciunt inventore.", 2 },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Tenetur adipisci recusandae.", 6 },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Molestiae est iusto.", 2 },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Numquam est labore.", 9 },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Inventore eum culpa.", 2 },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Ipsa corrupti distinctio.", 21 },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Hic perspiciatis quo.", 12 },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Fuga quis occaecati.", 7 },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Aut neque iusto.", 26 },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Dolorem perspiciatis placeat.", 7 },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Minima qui nobis.", 30 },
                    { 17, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Autem provident reprehenderit.", 27 },
                    { 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Nemo commodi distinctio.", 3 },
                    { 19, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Id tempore perferendis.", 14 },
                    { 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Et tempore eius.", 5 },
                    { 21, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Atque quo quam.", 13 },
                    { 22, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Consequuntur in adipisci.", 17 },
                    { 23, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Magnam voluptatem rerum.", 2 },
                    { 24, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Harum qui voluptate.", 16 },
                    { 25, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Et voluptas consequatur.", 14 },
                    { 26, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Voluptatem quidem repudiandae.", 13 },
                    { 27, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Impedit quasi magnam.", 8 },
                    { 28, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Ratione nostrum porro.", 13 },
                    { 29, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Quia laboriosam excepturi.", 15 },
                    { 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Neque magni voluptas.", 18 },
                    { 31, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Suscipit laboriosam qui.", 25 },
                    { 32, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Deleniti ratione illum.", 9 },
                    { 33, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Voluptatem iste laboriosam.", 1 },
                    { 34, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Libero et quaerat.", 23 },
                    { 35, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Et neque nulla.", 26 }
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
                    { 1, 11 },
                    { 2, 1 },
                    { 2, 11 },
                    { 3, 2 },
                    { 3, 9 },
                    { 4, 11 },
                    { 5, 2 },
                    { 5, 7 },
                    { 5, 12 },
                    { 6, 4 },
                    { 6, 7 },
                    { 7, 1 },
                    { 8, 3 },
                    { 8, 8 },
                    { 9, 1 },
                    { 9, 4 },
                    { 9, 9 },
                    { 10, 2 },
                    { 10, 5 },
                    { 11, 7 },
                    { 12, 7 },
                    { 12, 9 },
                    { 12, 10 },
                    { 12, 12 },
                    { 13, 2 },
                    { 13, 3 },
                    { 13, 6 },
                    { 13, 11 },
                    { 14, 2 },
                    { 14, 4 },
                    { 15, 7 },
                    { 16, 9 },
                    { 16, 11 },
                    { 17, 1 },
                    { 18, 4 },
                    { 18, 5 },
                    { 19, 10 },
                    { 20, 11 },
                    { 21, 5 },
                    { 21, 6 },
                    { 22, 7 },
                    { 22, 8 },
                    { 23, 2 },
                    { 23, 6 },
                    { 24, 1 },
                    { 25, 5 },
                    { 25, 8 },
                    { 26, 1 },
                    { 26, 3 },
                    { 27, 2 },
                    { 28, 9 },
                    { 29, 4 },
                    { 29, 10 },
                    { 30, 5 },
                    { 30, 9 },
                    { 31, 2 },
                    { 31, 5 },
                    { 32, 4 },
                    { 32, 6 },
                    { 33, 12 },
                    { 34, 4 },
                    { 34, 5 },
                    { 35, 11 },
                    { 36, 3 },
                    { 36, 9 },
                    { 37, 10 },
                    { 37, 11 },
                    { 38, 1 },
                    { 38, 4 },
                    { 38, 6 },
                    { 39, 4 },
                    { 39, 6 },
                    { 39, 7 },
                    { 40, 2 },
                    { 40, 9 }
                });

            migrationBuilder.InsertData(
                table: "CartItems",
                columns: new[] { "Id", "BookId", "CartId", "IsRemoved", "Quantity" },
                values: new object[,]
                {
                    { 1, 35, 1, false, 3 },
                    { 2, 29, 1, false, 2 },
                    { 3, 22, 1, false, 1 },
                    { 4, 2, 2, false, 2 },
                    { 5, 7, 2, false, 3 },
                    { 6, 37, 3, false, 3 },
                    { 7, 30, 4, false, 3 },
                    { 8, 11, 5, false, 3 },
                    { 9, 28, 7, false, 1 },
                    { 10, 1, 7, false, 3 },
                    { 11, 31, 8, false, 2 },
                    { 12, 25, 9, false, 1 },
                    { 13, 31, 9, false, 1 },
                    { 14, 27, 10, false, 1 },
                    { 15, 8, 10, false, 1 },
                    { 16, 32, 10, false, 1 },
                    { 17, 31, 11, false, 1 },
                    { 18, 2, 13, false, 2 }
                });

            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "Id", "AddedAt", "BookId", "IsRemoved", "OrderId", "Price", "Quantity" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, 1, 407.65298f, 1 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 38, false, 2, 168.76695f, 3 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 39, false, 3, 448.38287f, 1 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, false, 3, 289.71722f, 1 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 16, false, 3, 301.479f, 3 },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 13, false, 4, 426.5418f, 2 },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 31, false, 5, 371.26346f, 3 },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 34, false, 6, 166.49644f, 2 },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 36, false, 7, 377.5423f, 1 },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 35, false, 8, 428.66418f, 2 },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, 8, 225.36723f, 2 },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 19, false, 8, 373.91138f, 2 },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 39, false, 9, 448.38287f, 2 },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, 9, 407.65298f, 2 },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 30, false, 9, 292.2989f, 3 },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 18, false, 10, 284.8982f, 2 },
                    { 17, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, 10, 407.65298f, 3 },
                    { 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 34, false, 11, 166.49644f, 2 },
                    { 19, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 32, false, 11, 382.20273f, 3 },
                    { 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 26, false, 11, 261.0773f, 2 },
                    { 21, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 21, false, 12, 387.772f, 3 },
                    { 22, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20, false, 12, 178.34344f, 3 },
                    { 23, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 29, false, 12, 382.05847f, 2 },
                    { 24, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 22, false, 13, 158.22429f, 2 },
                    { 25, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 13, false, 13, 426.5418f, 1 },
                    { 26, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 37, false, 14, 341.96255f, 3 },
                    { 27, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, false, 15, 299.58423f, 3 },
                    { 28, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 33, false, 16, 325.28094f, 3 },
                    { 29, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, false, 16, 156.85414f, 2 },
                    { 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 17, false, 17, 208.20264f, 1 },
                    { 31, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 38, false, 18, 168.76695f, 1 },
                    { 32, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 30, false, 18, 292.2989f, 3 },
                    { 33, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 13, false, 18, 426.5418f, 3 },
                    { 34, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 35, false, 19, 428.66418f, 3 },
                    { 35, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 14, false, 20, 174.36795f, 3 },
                    { 36, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, false, 20, 182.33621f, 2 },
                    { 37, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, false, 20, 156.85414f, 3 }
                });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "Id", "BookId", "Comment", "CreatedAt", "IsRemoved", "Rating", "UserId" },
                values: new object[,]
                {
                    { 1, 7, "Quo consequatur vel facere ab nobis sunt.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 25 },
                    { 2, 37, "Aliquid nesciunt minus consequatur.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 14 },
                    { 3, 36, "Rem reiciendis eos doloribus voluptas qui.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 6 },
                    { 4, 38, "Sit modi et quas sunt consequatur voluptas quas porro.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 16 },
                    { 5, 35, "Et sed odio voluptatum repellat eos aut.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 23 },
                    { 6, 31, "Sit officia reiciendis sit sed ipsum unde quod natus.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 5 },
                    { 7, 30, "Nulla quasi qui non cum dolorem vitae.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 19 },
                    { 8, 36, "Accusamus in odit quas aut quam.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 16 },
                    { 9, 17, "Atque et accusantium dolorem delectus unde quia consequuntur doloremque.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 21 },
                    { 10, 29, "Et non velit qui rem deserunt eveniet ut eos modi.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 1 },
                    { 11, 6, "Quae recusandae eos.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 17 },
                    { 12, 13, "Ea similique cumque cumque quo dolorum officiis iure.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 28 },
                    { 13, 15, "Cupiditate deleniti sunt qui quo quia fugiat nisi id.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 4, 26 },
                    { 14, 12, "Eum corrupti neque consectetur saepe dicta qui sed.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 11 },
                    { 15, 11, "Aut omnis aut accusantium aut quis et dolorem.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 6 },
                    { 16, 25, "Ut ullam quo rem sed dicta.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 9 },
                    { 17, 12, "Aut magni quo.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 1 },
                    { 18, 39, "Facere laboriosam quia numquam est.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 6 },
                    { 19, 18, "Quisquam inventore velit et in.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 29 },
                    { 20, 37, "Deserunt autem molestiae culpa aspernatur quaerat quasi atque.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 4, 7 },
                    { 21, 36, "Est voluptas mollitia dolores cupiditate.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 26 },
                    { 22, 15, "Nisi enim laborum eius consequatur tenetur debitis in fuga.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 9 },
                    { 23, 10, "Consequatur saepe ea ipsum et hic qui.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 20 },
                    { 24, 4, "Consequatur aperiam qui.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 7 },
                    { 25, 7, "Non et et dolore odit enim commodi qui corporis adipisci.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 25 },
                    { 26, 10, "Exercitationem qui sapiente cumque sunt aliquid doloremque.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 28 },
                    { 27, 5, "Nihil mollitia numquam sit.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 4, 13 },
                    { 28, 3, "Quo cumque qui quod sed temporibus explicabo consequatur.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 7 },
                    { 29, 26, "Accusamus laboriosam ea impedit necessitatibus.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 16 },
                    { 30, 5, "Sint odit voluptatem sequi alias in quo.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 24 },
                    { 31, 16, "Corrupti deleniti molestias repudiandae.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 5 },
                    { 32, 17, "Repellat ut occaecati omnis dolorem aliquid vel consequatur consectetur quas.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 22 },
                    { 33, 19, "Modi quibusdam deserunt ut officia.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 14 },
                    { 34, 16, "Aut error vel consequatur iure fugiat quos alias.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 4 },
                    { 35, 38, "Maiores in sit.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 15 },
                    { 36, 33, "Cupiditate facilis a.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 4, 5 },
                    { 37, 8, "Quo voluptatem odio vel quisquam.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 19 },
                    { 38, 25, "Non ex perspiciatis asperiores provident optio.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 16 },
                    { 39, 39, "Nostrum labore in voluptatem sed repellendus et ea.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 3, 16 },
                    { 40, 1, "Dolorum cum consequuntur sed ipsum natus tempore sed est ducimus.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 26 },
                    { 41, 40, "Voluptatem et et dolorem qui consequatur aut blanditiis voluptas.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 4 },
                    { 42, 13, "Doloremque modi nesciunt omnis eos.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 1 },
                    { 43, 11, "Et esse non earum minima quia ab voluptas magni.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 4, 8 },
                    { 44, 6, "Accusamus eos doloremque officiis ratione quo dolorem.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 13 },
                    { 45, 5, "Labore rerum voluptatibus.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 5, 25 },
                    { 46, 28, "Nulla veritatis qui voluptas dicta ullam sed est.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 16 },
                    { 47, 17, "Voluptates repudiandae repellat molestias ex exercitationem minus doloribus odit odio.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, 24 },
                    { 48, 2, "Esse cumque rerum nisi occaecati sequi rerum.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 4, 17 },
                    { 49, 13, "Aut omnis minus tempore.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, 21 },
                    { 50, 3, "Numquam eveniet velit et dolores fuga ut tenetur aliquam.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 4, 13 }
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 1, 9 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 1, 17 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 1, 32 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 1, 37 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 1, 40 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 2, 5 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 2, 19 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 2, 22 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 2, 23 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 2, 29 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 3, 13 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 3, 18 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 3, 40 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 4, 20 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 4, 23 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 4, 28 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 4, 31 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 4, 38 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 5, 8 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 5, 32 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 5, 34 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 5, 35 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 6, 6 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 6, 8 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 6, 39 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 7, 12 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 7, 33 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 7, 39 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 8, 3 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 8, 7 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 8, 8 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 8, 15 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 8, 16 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 8, 37 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 9, 8 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 9, 10 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 9, 16 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 9, 19 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 9, 21 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 9, 29 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 9, 33 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 10, 9 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 10, 12 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 10, 16 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 11, 1 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 11, 6 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 11, 13 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 11, 25 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 11, 28 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 11, 38 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 12, 12 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 12, 36 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 13, 3 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 13, 11 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 13, 13 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 13, 28 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 13, 31 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 14, 3 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 14, 12 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 14, 16 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 14, 20 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 14, 39 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 15, 5 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 15, 22 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 15, 31 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 15, 34 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 15, 37 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 16, 1 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 16, 2 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 16, 14 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 16, 24 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 16, 26 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 17, 11 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 17, 25 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 17, 30 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 17, 34 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 17, 36 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 17, 38 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 18, 3 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 18, 32 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 18, 36 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 18, 38 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 19, 7 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 19, 35 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 20, 1 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 20, 2 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 20, 6 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 20, 14 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 20, 26 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 20, 27 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 20, 36 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 20, 38 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 21, 19 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 21, 22 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 22, 2 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 22, 7 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 22, 10 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 22, 24 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 22, 35 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 23, 4 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 23, 18 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 23, 39 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 24, 5 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 24, 27 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 25, 24 });

            migrationBuilder.DeleteData(
                table: "BookAuthor",
                keyColumns: new[] { "AuthorsId", "BooksId" },
                keyValues: new object[] { 25, 30 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 1, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 2, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 3, 2 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 3, 9 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 4, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 5, 2 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 5, 7 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 5, 12 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 6, 4 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 6, 7 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 7, 1 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 8, 3 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 8, 8 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 9, 1 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 9, 4 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 9, 9 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 10, 2 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 10, 5 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 11, 7 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 12, 7 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 12, 9 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 12, 10 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 12, 12 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 13, 2 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 13, 3 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 13, 6 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 13, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 14, 2 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 14, 4 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 15, 7 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 16, 9 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 16, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 17, 1 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 18, 4 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 18, 5 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 19, 10 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 20, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 21, 5 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 21, 6 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 22, 7 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 22, 8 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 23, 2 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 23, 6 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 24, 1 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 25, 5 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 25, 8 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 26, 1 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 26, 3 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 27, 2 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 28, 9 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 29, 4 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 29, 10 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 30, 5 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 30, 9 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 31, 2 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 31, 5 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 32, 4 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 32, 6 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 33, 12 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 34, 4 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 34, 5 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 35, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 36, 3 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 36, 9 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 37, 10 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 37, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 38, 1 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 38, 4 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 38, 6 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 39, 4 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 39, 6 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 39, 7 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 40, 2 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 40, 9 });

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "GiftCards",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "GiftCards",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "GiftCards",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 88);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 89);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 90);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 91);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 93);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 95);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 96);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 98);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "WishlistItems",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "GiftCards",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "GiftCards",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "GiftCards",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "GiftCards",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "GiftCards",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "GiftCards",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "GiftCards",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 30);
        }
    }
}
