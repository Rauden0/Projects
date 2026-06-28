using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class CartSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Carts",
                columns: new[] { "Id", "CreatedAt", "IsRemoved", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 19 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 16 },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 21 },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 4 },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 25 },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 10 },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 27 },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 26 },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 20 },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 12 },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1 },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 22 },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 13 },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 23 },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 8 },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 15 },
                    { 17, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 24 },
                    { 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 9 },
                    { 19, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2 },
                    { 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 30 },
                    { 21, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 6 }
                });

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 4,
                column: "Quantity",
                value: 2);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 5,
                column: "Quantity",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 9,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 10,
                column: "Quantity",
                value: 2);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 11,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 12,
                column: "Quantity",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 16,
                column: "Quantity",
                value: 2);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 18,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 19,
                column: "Quantity",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 20,
                column: "Quantity",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 21,
                column: "Quantity",
                value: 2);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 23,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 25,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 26,
                column: "Quantity",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 27,
                column: "Quantity",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 29,
                column: "Quantity",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 30,
                column: "Quantity",
                value: 2);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 32,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 33,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 34,
                column: "Quantity",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 36,
                column: "Quantity",
                value: 2);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 37,
                column: "Quantity",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Information", "Paid" },
                values: new object[] { "Quo voluptatem odio vel quisquam.", true });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Information", "OrderState", "Paid", "UserId" },
                values: new object[] { "Non ex perspiciatis asperiores provident.", 2, false, 16 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Information", "OrderState", "Paid", "TotalPrice", "UserId" },
                values: new object[] { "Id nostrum labore in voluptatem.", 2, false, 1988.163f, 21 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Information", "OrderState", "Paid", "PaymentMethod", "UserId" },
                values: new object[] { "Ea vero sit sequi itaque.", 3, true, 2, 4 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 5,
                column: "UserId",
                value: 19);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Information", "Paid", "TotalPrice" },
                values: new object[] { "Blanditiis voluptas voluptatem ipsam inventore.", true, 703.5012f });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Information", "OrderState", "TotalPrice", "UserId" },
                values: new object[] { "Omnis eos aliquam ad minus.", 1, 2211.7114f, 10 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Information", "OrderState", "PaymentMethod", "UserId" },
                values: new object[] { "Non earum minima quia ab.", 3, 2, 27 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Information", "OrderState", "TotalPrice", "UserId" },
                values: new object[] { "Quia cupiditate accusamus eos doloremque.", 1, 1325.4536f, 26 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Information", "PaymentMethod", "TotalPrice", "UserId" },
                values: new object[] { "Fugiat odit aut fugit labore.", 1, 1313.8652f, 27 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Information", "OrderState", "PaymentMethod", "TotalPrice", "UserId" },
                values: new object[] { "Nobis et id nulla veritatis.", 3, 2, 1842.6638f, 20 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "OrderState", "Paid", "TotalPrice", "UserId" },
                values: new object[] { 3, true, 2112.7354f, 12 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "OrderState", "TotalPrice" },
                values: new object[] { 3, 307.02307f });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "OrderState", "TotalPrice" },
                values: new object[] { 3, 417.03665f });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Information", "TotalPrice" },
                values: new object[] { "Nisi occaecati sequi rerum libero.", 1252.0278f });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Information", "OrderState", "Paid", "PaymentMethod", "TotalPrice", "UserId" },
                values: new object[] { "Omnis minus tempore nihil inventore.", 1, true, 1, 643.72723f, 10 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Information", "OrderState", "TotalPrice", "UserId" },
                values: new object[] { "Eveniet velit et dolores fuga.", 3, 2123.0947f, 23 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Information", "PaymentMethod", "TotalPrice" },
                values: new object[] { "Harum ea dolores ea aut.", 1, 406.94797f });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Information", "OrderState", "Paid", "PaymentMethod", "TotalPrice", "UserId" },
                values: new object[] { "Dicta iusto rerum nulla alias.", 3, true, 2, 2030.8478f, 15 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Animi consequuntur quasi.", 13 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Voluptates atque laborum.", 14 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Cum corrupti fugiat.", 10 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Eaque molestiae non.", 4 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Hic dolorem sit.", 8 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Expedita nesciunt est.", 2 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Eum est perferendis.", 25 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Aut et et.", 13 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Ullam soluta incidunt.", 3 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Sapiente hic voluptas.", 8 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Tempora sunt enim.", 20 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Nulla voluptas reiciendis.", 2 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Voluptas voluptate et.", 20 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Ad impedit placeat.", 10 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Iusto magnam et.", 14 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 16,
                column: "Name",
                value: "Totam quia sed.");

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Quo minus fugiat.", 19 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Eligendi minima veniam.", 11 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 19,
                column: "Name",
                value: "Ea optio consequatur.");

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Nulla est aut.", 3 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Autem aliquid beatae.", 30 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Qui suscipit qui.", 22 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Non autem vel.", 18 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Suscipit et dolorem.", 22 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Consequatur cumque sed.", 1 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Aut dolorum ut.", 4 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 27,
                column: "Name",
                value: "Consequuntur molestiae repudiandae.");

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Ut nam mollitia.", 18 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Ad et nisi.", 24 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Consequuntur et rem.", 4 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Repudiandae et molestiae.", 16 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Laudantium sunt nam.", 28 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Non temporibus porro.", 24 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Ducimus eos reiciendis.", 5 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Eum doloremque culpa.", 6 });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 4,
                column: "Quantity",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 5,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 9,
                column: "Quantity",
                value: 2);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 10,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 11,
                column: "Quantity",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 12,
                column: "Quantity",
                value: 2);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 16,
                column: "Quantity",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 18,
                column: "Quantity",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 19,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 20,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 21,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 23,
                column: "Quantity",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 25,
                column: "Quantity",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 26,
                column: "Quantity",
                value: 2);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 27,
                column: "Quantity",
                value: 2);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 29,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 30,
                column: "Quantity",
                value: 1);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 32,
                column: "Quantity",
                value: 2);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 33,
                column: "Quantity",
                value: 2);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 34,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 36,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: 37,
                column: "Quantity",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Information", "Paid" },
                values: new object[] { "Quaerat quo voluptatem odio vel.", false });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Information", "OrderState", "Paid", "UserId" },
                values: new object[] { "Voluptate non ex perspiciatis asperiores.", 3, true, 22 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Information", "OrderState", "Paid", "TotalPrice", "UserId" },
                values: new object[] { "Non id nostrum labore in.", 3, true, 2530.9922f, 17 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Information", "OrderState", "Paid", "PaymentMethod", "UserId" },
                values: new object[] { "Et ea vero sit sequi.", 1, false, 3, 8 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 5,
                column: "UserId",
                value: 29);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Information", "Paid", "TotalPrice" },
                values: new object[] { "Aut blanditiis voluptas voluptatem ipsam.", false, 469.0008f });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Information", "OrderState", "TotalPrice", "UserId" },
                values: new object[] { "Nesciunt omnis eos aliquam ad.", 2, 2288.4412f, 2 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Information", "OrderState", "PaymentMethod", "UserId" },
                values: new object[] { "Esse non earum minima quia.", 4, 3, 23 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Information", "OrderState", "TotalPrice", "UserId" },
                values: new object[] { "Voluptate ratione quia cupiditate accusamus.", 4, 971.9717f, 2 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Information", "PaymentMethod", "TotalPrice", "UserId" },
                values: new object[] { "Ratione quo dolorem fugiat odit.", 3, 2012.749f, 26 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Information", "OrderState", "PaymentMethod", "TotalPrice", "UserId" },
                values: new object[] { "Provident nobis et id nulla.", 2, 3, 1466.7849f, 25 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "OrderState", "Paid", "TotalPrice", "UserId" },
                values: new object[] { 2, false, 1498.6584f, 2 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "OrderState", "TotalPrice" },
                values: new object[] { 4, 614.04614f });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "OrderState", "TotalPrice" },
                values: new object[] { 4, 834.0733f });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Information", "TotalPrice" },
                values: new object[] { "Occaecati sequi rerum libero corporis.", 2013.6384f });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Information", "OrderState", "Paid", "PaymentMethod", "TotalPrice", "UserId" },
                values: new object[] { "Tempore nihil inventore omnis fugiat.", 2, false, 3, 321.86362f, 8 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Information", "OrderState", "TotalPrice", "UserId" },
                values: new object[] { "Et dolores fuga ut tenetur.", 4, 1554.3079f, 7 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Information", "PaymentMethod", "TotalPrice" },
                values: new object[] { "Ea aut voluptatum rerum et.", 2, 1220.8439f });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Information", "OrderState", "Paid", "PaymentMethod", "TotalPrice", "UserId" },
                values: new object[] { "Alias et laborum minus vel.", 2, false, 3, 2840.7324f, 3 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Hic voluptatem est.", 29 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Totam voluptas libero.", 5 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Culpa porro animi.", 2 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Quis quia quo.", 20 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Est sequi eius.", 29 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Ratione ut omnis.", 5 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Eius voluptas inventore.", 28 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Hic placeat nisi.", 19 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Dolore optio dicta.", 18 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Beatae et et.", 27 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Dolor magni possimus.", 17 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Ad nisi voluptas.", 28 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Debitis perspiciatis voluptatem.", 14 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Et et temporibus.", 7 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Voluptatem voluptas itaque.", 19 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 16,
                column: "Name",
                value: "Nihil quae sunt.");

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Autem doloremque qui.", 25 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Sed quo rerum.", 21 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 19,
                column: "Name",
                value: "Non soluta aut.");

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Officia vero et.", 1 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Corrupti est placeat.", 27 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Animi consequuntur quasi.", 13 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Voluptates atque laborum.", 14 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Cum corrupti fugiat.", 10 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Eaque molestiae non.", 4 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Hic dolorem sit.", 8 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 27,
                column: "Name",
                value: "Expedita nesciunt est.");

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Eum est perferendis.", 25 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Aut et et.", 13 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Ullam soluta incidunt.", 3 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Sapiente hic voluptas.", 8 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Tempora sunt enim.", 20 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Nulla voluptas reiciendis.", 2 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Voluptas voluptate et.", 20 });

            migrationBuilder.UpdateData(
                table: "Wishlists",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Ad impedit placeat.", 10 });
        }
    }
}
