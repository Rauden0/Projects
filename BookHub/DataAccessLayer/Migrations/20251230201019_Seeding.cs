using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class Seeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Coupon_AppliedCouponId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Coupon_GiftCard_GiftCardId",
                table: "Coupon");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GiftCard",
                table: "GiftCard");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Coupon",
                table: "Coupon");

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
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 1, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 2, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 3, 11 });

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
                keyValues: new object[] { 5, 6 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 5, 9 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 6, 2 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 7, 10 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 7, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 8, 7 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 8, 8 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 8, 12 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 9, 1 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 9, 9 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 10, 4 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 10, 7 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 11, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 12, 1 });

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
                keyValues: new object[] { 13, 3 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 13, 5 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 13, 10 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 13, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 14, 9 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 14, 12 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 15, 4 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 16, 5 });

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
                keyValues: new object[] { 17, 3 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 18, 5 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 19, 5 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 19, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 20, 7 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 20, 9 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 21, 3 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 22, 2 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 22, 6 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 23, 8 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 24, 2 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 24, 4 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 25, 4 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 25, 5 });

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
                keyValues: new object[] { 27, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 28, 6 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 29, 1 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 30, 2 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 30, 7 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 31, 5 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 32, 1 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 32, 8 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 33, 1 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 33, 7 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 34, 5 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 34, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 35, 8 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 35, 11 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 36, 5 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 37, 7 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 37, 10 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 38, 4 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 38, 7 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 38, 10 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 39, 2 });

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
                keyValues: new object[] { 39, 9 });

            migrationBuilder.DeleteData(
                table: "BookGenre",
                keyColumns: new[] { "BooksId", "GenresId" },
                keyValues: new object[] { 40, 7 });

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
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "CartItems",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 17);

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
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 8);

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
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 8);

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
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 21);

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
                keyValue: 8);

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
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 17);

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
                keyValue: 7);

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
                keyValue: 13);

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
                keyValue: 4);

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

            migrationBuilder.RenameTable(
                name: "GiftCard",
                newName: "GiftCards");

            migrationBuilder.RenameTable(
                name: "Coupon",
                newName: "Coupons");

            migrationBuilder.RenameIndex(
                name: "IX_Coupon_GiftCardId",
                table: "Coupons",
                newName: "IX_Coupons_GiftCardId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GiftCards",
                table: "GiftCards",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Coupons",
                table: "Coupons",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Coupons_AppliedCouponId",
                table: "Carts",
                column: "AppliedCouponId",
                principalTable: "Coupons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Coupons_GiftCards_GiftCardId",
                table: "Coupons",
                column: "GiftCardId",
                principalTable: "GiftCards",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Coupons_AppliedCouponId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Coupons_GiftCards_GiftCardId",
                table: "Coupons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GiftCards",
                table: "GiftCards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Coupons",
                table: "Coupons");

            migrationBuilder.RenameTable(
                name: "GiftCards",
                newName: "GiftCard");

            migrationBuilder.RenameTable(
                name: "Coupons",
                newName: "Coupon");

            migrationBuilder.RenameIndex(
                name: "IX_Coupons_GiftCardId",
                table: "Coupon",
                newName: "IX_Coupon_GiftCardId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GiftCard",
                table: "GiftCard",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Coupon",
                table: "Coupon",
                column: "Id");

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
                columns: new[] { "Id", "CreatedAt", "Description", "ImagePath", "IsRemoved", "Name", "Price", "PrimaryGenreId", "PublisherId", "StockQuantity" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Minima quasi ipsam doloribus quis odit exercitationem placeat et neque voluptas iste.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Quam eum ullam.", 345.6067f, null, 9, 20 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Nemo dolores dolorem dolorem minus asperiores et atque dolorem praesentium dolores optio.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Est expedita optio.", 202.88036f, null, 11, 20 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ut fugit cumque error neque eius aut veniam porro omnis omnis totam.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Quia et beatae.", 238.0615f, null, 10, 20 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quibusdam quis distinctio dolores unde architecto est molestias praesentium facilis quibusdam illo.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Sunt pariatur quidem.", 417.03665f, null, 7, 20 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Amet dolores tenetur dolores ratione repudiandae nesciunt aut sit odio accusamus perferendis.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Porro et in.", 309.2449f, null, 13, 20 },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Recusandae quo et rerum et eaque aut illo cumque in numquam incidunt.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Molestiae pariatur ut.", 398.1443f, null, 2, 20 },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Et beatae saepe et est quis quas magnam provident quidem qui quos.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Quae illum porro.", 429.07956f, null, 13, 20 },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Et debitis velit et eum blanditiis rerum beatae ducimus laboriosam earum et.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Officiis quia ut.", 206.69057f, null, 5, 20 },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Velit nulla tenetur ipsa veniam totam id eligendi enim ea amet error.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Qui velit vel.", 380.80524f, null, 12, 20 },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dolorum ipsum voluptate sunt quae dolorum hic quam sed doloremque error mollitia.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Nisi voluptatem deleniti.", 252.60945f, null, 5, 20 },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reprehenderit accusantium magnam nemo numquam voluptate cum recusandae fugit illo possimus excepturi.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Repellat nostrum sed.", 353.52042f, null, 9, 20 },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Optio et autem est commodi modi consequatur dolorum vitae iusto ipsum aspernatur.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Animi et impedit.", 428.59583f, null, 2, 20 },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Vel ipsam est ipsum voluptate omnis minus sequi recusandae error consectetur et.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Amet ad et.", 307.03854f, null, 7, 20 },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Et ut et qui doloremque et et nobis magni aut quas quo.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Sapiente delectus harum.", 205.53908f, null, 15, 20 },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Amet veniam vero ut nostrum saepe veritatis similique molestiae expedita vel quo.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Similique labore accusantium.", 317.64792f, null, 15, 20 },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rem voluptas est aliquam aliquid excepturi non atque ad temporibus tempora ipsam.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Quia magni exercitationem.", 374.75998f, null, 9, 20 },
                    { 17, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Minima soluta odit explicabo voluptas qui iusto dolorem id qui molestiae aut.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Fuga libero dolores.", 321.86362f, null, 1, 20 },
                    { 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dolores est laudantium est fugiat assumenda sit illo perferendis odit fugiat aliquam.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Exercitationem impedit necessitatibus.", 353.48187f, null, 1, 20 },
                    { 19, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Eligendi non voluptatem sed qui reprehenderit laboriosam et placeat quis aspernatur excepturi.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Consectetur quae veritatis.", 360.99533f, null, 12, 20 },
                    { 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Aut eveniet sed est dignissimos voluptates omnis eaque enim ut animi similique.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Recusandae est eos.", 427.28583f, null, 2, 20 },
                    { 21, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Enim voluptas nobis nobis ex suscipit consequuntur laboriosam dicta in ut ut.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Labore molestiae excepturi.", 243.30276f, null, 5, 20 },
                    { 22, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "A nostrum nulla laboriosam dolore dolorum dolore dolorem fugiat ipsa id eos.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Ut est illum.", 397.20663f, null, 5, 20 },
                    { 23, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Voluptatum dolorem et dolor maxime error architecto eum ex molestias perspiciatis et.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Vero qui adipisci.", 150.34575f, null, 1, 20 },
                    { 24, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Consequuntur hic vitae ipsum veritatis ratione odio occaecati cupiditate nostrum reprehenderit deleniti.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Cumque et odit.", 228.833f, null, 2, 20 },
                    { 25, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Voluptas id est rerum non est est tenetur necessitatibus aut nihil nam.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Autem tenetur sed.", 399.09235f, null, 1, 20 },
                    { 26, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Neque voluptates quas voluptatem esse sit atque expedita modi enim soluta et.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Aperiam earum porro.", 178.2647f, null, 6, 20 },
                    { 27, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Amet sapiente adipisci totam repudiandae modi enim eius ipsum eaque minus at.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Dolorem perspiciatis aut.", 357.67044f, null, 5, 20 },
                    { 28, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tenetur laudantium ducimus aspernatur nihil quisquam et voluptatibus doloribus blanditiis sunt ab.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Ex ut perspiciatis.", 389.13806f, null, 11, 20 },
                    { 29, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dolorem architecto maxime magnam dolore est adipisci et dolore voluptatibus placeat aut.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Sed quia rerum.", 309.59082f, null, 15, 20 },
                    { 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Odio cumque explicabo repellendus voluptas et hic atque molestiae voluptas eius ipsum.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Voluptas consequatur unde.", 261.74844f, null, 5, 20 },
                    { 31, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Blanditiis veniam autem exercitationem nihil omnis aliquam exercitationem dolor id dolores quia.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Velit minus maxime.", 286.865f, null, 7, 20 },
                    { 32, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ad fugiat accusantium nesciunt veritatis eum quibusdam voluptas quia et aut harum.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Voluptas nobis dolore.", 412.28302f, null, 10, 20 },
                    { 33, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sed ipsa sunt magnam id molestiae voluptas quos qui molestias blanditiis nemo.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Omnis architecto repellendus.", 290.40756f, null, 5, 20 },
                    { 34, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ipsa commodi nihil qui qui blanditiis repellat provident vel at minima distinctio.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Blanditiis atque quia.", 241.10587f, null, 2, 20 },
                    { 35, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Non quisquam aut repellat ut voluptates amet libero voluptatum quia laboriosam blanditiis.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Earum aut libero.", 406.94797f, null, 10, 20 },
                    { 36, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Et consectetur quis sed qui officia in quisquam ex quasi natus fugiat.", "/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png", false, "Occaecati voluptatem placeat.", 234.5004f, null, 7, 20 },
                    { 37, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Autem minus autem consequuntur quas qui sint sint sint magni expedita beatae.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Sint quia quibusdam.", 307.02307f, null, 4, 20 },
                    { 38, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Blanditiis facere minus totam repellendus dolores provident ad sit quo fuga fugiat.", "/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png", false, "Possimus excepturi omnis.", 208.36693f, null, 14, 20 },
                    { 39, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Et temporibus est fuga fugit id at quis tempora blanditiis sint rerum.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Dolorem dolore inventore.", 400.0073f, null, 13, 20 },
                    { 40, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Aut voluptas voluptatum sit officia mollitia voluptas pariatur quia et necessitatibus vitae.", "/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png", false, "Aut quia qui.", 366.8537f, null, 4, 20 }
                });

            migrationBuilder.InsertData(
                table: "Carts",
                columns: new[] { "Id", "AppliedCouponId", "CreatedAt", "IsRemoved", "UserId" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 19 },
                    { 2, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 16 },
                    { 3, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 21 },
                    { 4, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 4 },
                    { 5, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 25 },
                    { 6, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 10 },
                    { 7, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 27 },
                    { 8, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 26 },
                    { 9, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 20 },
                    { 10, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 12 },
                    { 11, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1 },
                    { 12, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 22 },
                    { 13, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 13 },
                    { 14, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 23 },
                    { 15, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 8 },
                    { 16, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 15 },
                    { 17, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 24 },
                    { 18, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 9 },
                    { 19, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2 },
                    { 20, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 30 },
                    { 21, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 6 }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "CreatedAt", "Information", "IsRemoved", "OrderState", "Paid", "PaymentMethod", "TotalPrice", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quo voluptatem odio vel quisquam.", false, 1, true, 1, 309.2449f, 19 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Non ex perspiciatis asperiores provident.", false, 2, false, 3, 416.73386f, 16 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Id nostrum labore in voluptatem.", false, 2, false, 3, 1988.163f, 21 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ea vero sit sequi itaque.", false, 3, true, 2, 614.0771f, 4 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sed ipsum natus tempore sed.", false, 3, true, 1, 860.595f, 19 },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sed autem voluptatem et et.", false, 2, true, 3, 241.10587f, 21 },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Blanditiis voluptas voluptatem ipsam inventore.", false, 1, true, 1, 703.5012f, 25 },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Omnis eos aliquam ad minus.", false, 1, true, 1, 2211.7114f, 10 },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Non earum minima quia ab.", false, 3, true, 2, 1989.4977f, 27 },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quia cupiditate accusamus eos doloremque.", false, 1, true, 1, 1325.4536f, 26 },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fugiat odit aut fugit labore.", false, 1, false, 1, 1313.8652f, 27 },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Nobis et id nulla veritatis.", false, 3, true, 2, 1842.6638f, 20 },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ullam sed est omnis ea.", false, 3, true, 1, 2112.7354f, 12 },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Repudiandae repellat molestias ex exercitationem.", false, 3, true, 3, 307.02307f, 1 },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Odio similique aperiam possimus est.", false, 3, true, 1, 417.03665f, 22 },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Nisi occaecati sequi rerum libero.", false, 3, true, 2, 1252.0278f, 13 },
                    { 17, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Omnis minus tempore nihil inventore.", false, 1, true, 1, 643.72723f, 10 },
                    { 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Eveniet velit et dolores fuga.", false, 3, true, 1, 2123.0947f, 23 },
                    { 19, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Harum ea dolores ea aut.", false, 3, true, 1, 406.94797f, 8 },
                    { 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dicta iusto rerum nulla alias.", false, 3, true, 2, 2030.8478f, 15 }
                });

            migrationBuilder.InsertData(
                table: "Wishlists",
                columns: new[] { "Id", "CreatedAt", "IsRemoved", "Name", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Animi consequuntur quasi.", 13 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Voluptates atque laborum.", 14 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Cum corrupti fugiat.", 10 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Eaque molestiae non.", 4 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Hic dolorem sit.", 8 },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Expedita nesciunt est.", 2 },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Eum est perferendis.", 25 },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Aut et et.", 13 },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Ullam soluta incidunt.", 3 },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Sapiente hic voluptas.", 8 },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Tempora sunt enim.", 20 },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Nulla voluptas reiciendis.", 2 },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Voluptas voluptate et.", 20 },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Ad impedit placeat.", 10 },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Iusto magnam et.", 14 },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Totam quia sed.", 5 },
                    { 17, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Quo minus fugiat.", 19 },
                    { 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Eligendi minima veniam.", 11 },
                    { 19, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Ea optio consequatur.", 30 },
                    { 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Nulla est aut.", 3 },
                    { 21, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Autem aliquid beatae.", 30 },
                    { 22, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Qui suscipit qui.", 22 },
                    { 23, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Non autem vel.", 18 },
                    { 24, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Suscipit et dolorem.", 22 },
                    { 25, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Consequatur cumque sed.", 1 },
                    { 26, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Aut dolorum ut.", 4 },
                    { 27, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Consequuntur molestiae repudiandae.", 2 },
                    { 28, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Ut nam mollitia.", 18 },
                    { 29, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Ad et nisi.", 24 },
                    { 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Consequuntur et rem.", 4 },
                    { 31, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Repudiandae et molestiae.", 16 },
                    { 32, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Laudantium sunt nam.", 28 },
                    { 33, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Non temporibus porro.", 24 },
                    { 34, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Ducimus eos reiciendis.", 5 },
                    { 35, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Eum doloremque culpa.", 6 }
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
                table: "CartItems",
                columns: new[] { "Id", "BookId", "CartId", "IsRemoved", "Quantity" },
                values: new object[,]
                {
                    { 1, 1, 1, false, 3 },
                    { 2, 7, 1, false, 2 },
                    { 3, 31, 1, false, 3 },
                    { 4, 29, 3, false, 2 },
                    { 5, 26, 3, false, 2 },
                    { 6, 39, 5, false, 2 },
                    { 7, 7, 5, false, 1 },
                    { 8, 31, 8, false, 3 },
                    { 9, 3, 10, false, 2 },
                    { 10, 30, 11, false, 2 },
                    { 11, 23, 11, false, 1 },
                    { 12, 28, 11, false, 1 },
                    { 13, 4, 12, false, 2 },
                    { 14, 26, 12, false, 2 },
                    { 15, 8, 12, false, 1 },
                    { 16, 37, 13, false, 1 },
                    { 17, 14, 13, false, 3 },
                    { 18, 18, 13, false, 3 },
                    { 19, 6, 14, false, 1 },
                    { 20, 20, 14, false, 3 },
                    { 21, 25, 15, false, 1 },
                    { 22, 31, 15, false, 3 },
                    { 23, 6, 15, false, 3 },
                    { 24, 16, 18, false, 2 },
                    { 25, 27, 18, false, 1 },
                    { 26, 36, 19, false, 3 },
                    { 27, 22, 19, false, 3 },
                    { 28, 39, 19, false, 1 },
                    { 29, 35, 20, false, 3 },
                    { 30, 25, 21, false, 3 }
                });

            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "Id", "AddedAt", "BookId", "IsRemoved", "OrderId", "Price", "Quantity" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, 1, 309.2449f, 1 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 38, false, 2, 208.36693f, 2 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 39, false, 3, 400.0073f, 3 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, false, 3, 206.69057f, 2 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 16, false, 3, 374.75998f, 1 },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 13, false, 4, 307.03854f, 2 },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 31, false, 5, 286.865f, 3 },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 34, false, 6, 241.10587f, 1 },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 36, false, 7, 234.5004f, 3 },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 35, false, 8, 406.94797f, 2 },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, 8, 345.6067f, 3 },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 19, false, 8, 360.99533f, 1 },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 39, false, 9, 400.0073f, 2 },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, 9, 309.2449f, 3 },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 30, false, 9, 261.74844f, 1 },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 18, false, 10, 353.48187f, 2 },
                    { 17, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, 10, 309.2449f, 2 },
                    { 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 34, false, 11, 241.10587f, 3 },
                    { 19, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 32, false, 11, 412.28302f, 1 },
                    { 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 26, false, 11, 178.2647f, 1 },
                    { 21, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 21, false, 12, 243.30276f, 2 },
                    { 22, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20, false, 12, 427.28583f, 1 },
                    { 23, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 29, false, 12, 309.59082f, 3 },
                    { 24, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 22, false, 13, 397.20663f, 3 },
                    { 25, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 13, false, 13, 307.03854f, 3 },
                    { 26, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 37, false, 14, 307.02307f, 1 },
                    { 27, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, false, 15, 417.03665f, 1 },
                    { 28, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 33, false, 16, 290.40756f, 3 },
                    { 29, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, false, 16, 380.80524f, 1 },
                    { 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 17, false, 17, 321.86362f, 2 },
                    { 31, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 38, false, 18, 208.36693f, 2 },
                    { 32, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 30, false, 18, 261.74844f, 3 },
                    { 33, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 13, false, 18, 307.03854f, 3 },
                    { 34, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 35, false, 19, 406.94797f, 1 },
                    { 35, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 14, false, 20, 205.53908f, 2 },
                    { 36, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, false, 20, 429.07956f, 2 },
                    { 37, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, false, 20, 380.80524f, 2 }
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

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Coupon_AppliedCouponId",
                table: "Carts",
                column: "AppliedCouponId",
                principalTable: "Coupon",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Coupon_GiftCard_GiftCardId",
                table: "Coupon",
                column: "GiftCardId",
                principalTable: "GiftCard",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
