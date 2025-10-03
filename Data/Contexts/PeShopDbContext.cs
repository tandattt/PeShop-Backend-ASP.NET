using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PeShop.Models.Entities;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace PeShop.Data.Contexts;

public partial class PeShopDbContext : DbContext
{
    public PeShopDbContext()
    {
    }

    public PeShopDbContext(DbContextOptions<PeShopDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AttributeTemplate> AttributeTemplates { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryChild> CategoryChildren { get; set; }

    public virtual DbSet<CoreEnum> CoreEnums { get; set; }

    public virtual DbSet<ImageProduct> ImageProducts { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<OrderVoucher> OrderVouchers { get; set; }

    public virtual DbSet<Payout> Payouts { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductInfomation> ProductInfomations { get; set; }

    public virtual DbSet<PropertyProduct> PropertyProducts { get; set; }

    public virtual DbSet<PropertyValue> PropertyValues { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Shop> Shops { get; set; }

    public virtual DbSet<TemplateRegisterCategory> TemplateRegisterCategories { get; set; }

    public virtual DbSet<TemplateRegisterCategoryChild> TemplateRegisterCategoryChildren { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAddress> UserAddresses { get; set; }

    public virtual DbSet<Variant> Variants { get; set; }

    public virtual DbSet<VariantValue> VariantValues { get; set; }

    public virtual DbSet<VoucherShop> VoucherShops { get; set; }

    public virtual DbSet<VoucherSystem> VoucherSystems { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AttributeTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("attribute_template");

            entity.HasIndex(e => e.TemplateRegisterCategoryChildId, "FK1yyblerqp9v0jr44kcn2oix1s");

            entity.HasIndex(e => e.TemplateRegisterCategoryId, "FKeopegh7t3sx4p8pveldrlr6pm");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
            entity.Property(e => e.TemplateRegisterCategoryChildId).HasColumnName("template_register_category_child_id");
            entity.Property(e => e.TemplateRegisterCategoryId).HasColumnName("template_register_category_id");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");

            entity.HasOne(d => d.TemplateRegisterCategoryChild).WithMany(p => p.AttributeTemplates)
                .HasForeignKey(d => d.TemplateRegisterCategoryChildId)
                .HasConstraintName("FK1yyblerqp9v0jr44kcn2oix1s");

            entity.HasOne(d => d.TemplateRegisterCategory).WithMany(p => p.AttributeTemplates)
                .HasForeignKey(d => d.TemplateRegisterCategoryId)
                .HasConstraintName("FKeopegh7t3sx4p8pveldrlr6pm");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("cart");

            entity.HasIndex(e => e.ProductId, "FK3d704slv66tw6x5hmbm6p2x3u");

            entity.HasIndex(e => e.VariantId, "FKiooeaakvn6wca6h281k8m881g");

            entity.HasIndex(e => e.UserId, "FKl70asp4l4w0jmbm1tqyofho4o");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductId)
                .HasMaxLength(36)
                .HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");
            entity.Property(e => e.VariantId).HasColumnName("variant_id");

            entity.HasOne(d => d.Product).WithMany(p => p.Carts)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK3d704slv66tw6x5hmbm6p2x3u");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FKl70asp4l4w0jmbm1tqyofho4o");

            entity.HasOne(d => d.Variant).WithMany(p => p.Carts)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FKiooeaakvn6wca6h281k8m881g");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("category");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<CategoryChild>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("category_child");

            entity.HasIndex(e => e.CategoryId, "FKhtposlpk6ai3ctf1xu8jfcm8u");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.CategoryId)
                .HasMaxLength(36)
                .HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryChildren)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FKhtposlpk6ai3ctf1xu8jfcm8u");
        });

        modelBuilder.Entity<CoreEnum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("core_enum");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<ImageProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("image_product");

            entity.HasIndex(e => e.ProductId, "FKml4177k7ufupebm7e4wgmvpnj");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.ProductId)
                .HasMaxLength(36)
                .HasColumnName("product_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .HasColumnName("url");

            entity.HasOne(d => d.Product).WithMany(p => p.ImageProducts)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FKml4177k7ufupebm7e4wgmvpnj");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("orders");

            entity.HasIndex(e => e.UserId, "FKel9kyl84ego2otj2accfd8mr7");

            entity.HasIndex(e => e.ShopId, "FKqn03kko0738sehaal2gr2uxl6");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.DeliveryAddress)
                .HasMaxLength(255)
                .HasColumnName("delivery_address");
            entity.Property(e => e.DeliveryStatus).HasColumnName("delivery_status");
            entity.Property(e => e.FinalPrice)
                .HasPrecision(18, 3)
                .HasColumnName("final_price");
            entity.Property(e => e.OriginalPrice)
                .HasPrecision(18, 3)
                .HasColumnName("original_price");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method");
            entity.Property(e => e.ShopId)
                .HasMaxLength(36)
                .HasColumnName("shop_id");
            entity.Property(e => e.StatusOrder).HasColumnName("status_order");
            entity.Property(e => e.StatusPayment).HasColumnName("status_payment");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");

            entity.HasOne(d => d.Shop).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("FKqn03kko0738sehaal2gr2uxl6");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FKel9kyl84ego2otj2accfd8mr7");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("order_detail");

            entity.HasIndex(e => e.ProductId, "FKb8bg2bkty0oksa3wiq5mp5qnc");

            entity.HasIndex(e => e.VariantId, "FKbxjrinr4c8w8k5gjn7qtkod70");

            entity.HasIndex(e => e.OrderId, "FKrws2q0si6oyd6il8gqe2aennc");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.DiscountPrice)
                .HasPrecision(18, 3)
                .HasColumnName("discount_price");
            entity.Property(e => e.Note)
                .HasMaxLength(300)
                .HasColumnName("note");
            entity.Property(e => e.OrderId)
                .HasMaxLength(36)
                .HasColumnName("order_id");
            entity.Property(e => e.OriginalPrice)
                .HasPrecision(18, 3)
                .HasColumnName("original_price");
            entity.Property(e => e.ProductId)
                .HasMaxLength(36)
                .HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ShippingFee)
                .HasPrecision(10, 2)
                .HasColumnName("shipping_fee");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
            entity.Property(e => e.VariantId).HasColumnName("variant_id");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FKrws2q0si6oyd6il8gqe2aennc");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FKb8bg2bkty0oksa3wiq5mp5qnc");

            entity.HasOne(d => d.Variant).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FKbxjrinr4c8w8k5gjn7qtkod70");
        });

        modelBuilder.Entity<OrderVoucher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("order_voucher");

            entity.HasIndex(e => e.OrderDetailId, "FKfkbsbbmmayevbbainsbk30mk0");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.OrderDetailId)
                .HasMaxLength(36)
                .HasColumnName("order_detail_id");
            entity.Property(e => e.PriceVoucherShop)
                .HasPrecision(18, 3)
                .HasColumnName("price_voucher_shop");
            entity.Property(e => e.PriceVoucherSystem)
                .HasPrecision(18, 3)
                .HasColumnName("price_voucher_system");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
            entity.Property(e => e.VoucherShopName)
                .HasMaxLength(45)
                .HasColumnName("voucher_shop_name");
            entity.Property(e => e.VoucherSystemName)
                .HasMaxLength(45)
                .HasColumnName("voucher_system_name");

            entity.HasOne(d => d.OrderDetail).WithMany(p => p.OrderVouchers)
                .HasForeignKey(d => d.OrderDetailId)
                .HasConstraintName("FKfkbsbbmmayevbbainsbk30mk0");
        });

        modelBuilder.Entity<Payout>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("payouts");

            entity.HasIndex(e => e.OrderId, "FK2vu4iksho3871wts97icg5d8b");

            entity.HasIndex(e => e.ShopId, "FKepoaje1tfo0x60x9u0hysqkgk");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.GrossAmount)
                .HasPrecision(15, 2)
                .HasColumnName("gross_amount");
            entity.Property(e => e.NetAmount)
                .HasPrecision(15, 2)
                .HasColumnName("net_amount");
            entity.Property(e => e.OrderId)
                .HasMaxLength(36)
                .HasColumnName("order_id");
            entity.Property(e => e.PaidAt)
                .HasMaxLength(6)
                .HasColumnName("paid_at");
            entity.Property(e => e.PlatformFee)
                .HasPrecision(15, 2)
                .HasColumnName("platform_fee");
            entity.Property(e => e.ShippingFee)
                .HasPrecision(15, 2)
                .HasColumnName("shipping_fee");
            entity.Property(e => e.ShopId)
                .HasMaxLength(36)
                .HasColumnName("shop_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");

            entity.HasOne(d => d.Order).WithMany(p => p.Payouts)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK2vu4iksho3871wts97icg5d8b");

            entity.HasOne(d => d.Shop).WithMany(p => p.Payouts)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("FKepoaje1tfo0x60x9u0hysqkgk");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("product");

            entity.HasIndex(e => e.ShopId, "FK94hgg8hlqfqfnt3dag950vm7n");

            entity.HasIndex(e => e.CategoryChildId, "FKj80e6pnpa9do0rkadbntug0hl");


            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.BoughtCount).HasColumnName("bought_count");
            entity.Property(e => e.CategoryChildId)
                .HasMaxLength(36)
                .HasColumnName("category_child_id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.ImgMain)
                .HasMaxLength(255)
                .HasColumnName("img_main");
            entity.Property(e => e.Length).HasColumnName("length");
            entity.Property(e => e.LikeCount).HasColumnName("like_count");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ReviewCount).HasColumnName("review_count");
            entity.Property(e => e.ReviewPoint).HasColumnName("review_point");
            entity.Property(e => e.ShopId)
                .HasMaxLength(36)
                .HasColumnName("shop_id");
            entity.Property(e => e.Slug)
                .HasMaxLength(255)
                .HasColumnName("slug");
            entity.Property(e => e.Status)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
            entity.Property(e => e.ViewCount).HasColumnName("view_count");
            entity.Property(e => e.Weight).HasColumnName("weight");
            entity.Property(e => e.Width).HasColumnName("width");

            entity.HasOne(d => d.CategoryChild).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryChildId)
                .HasConstraintName("FKj80e6pnpa9do0rkadbntug0hl");

            entity.HasOne(d => d.Shop).WithMany(p => p.Products)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("FK94hgg8hlqfqfnt3dag950vm7n");

        });

        modelBuilder.Entity<ProductInfomation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("product_infomation");

            entity.HasIndex(e => e.ProductId, "FK4sb3w74gm2nqxtoni3ti4yg86");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ProductId)
                .HasMaxLength(36)
                .HasColumnName("product_id");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
            entity.Property(e => e.Value)
                .HasMaxLength(255)
                .HasColumnName("value");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductInfomations)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK4sb3w74gm2nqxtoni3ti4yg86");
        });

        modelBuilder.Entity<PropertyProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("property_product");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<PropertyValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("property_value");

            entity.HasIndex(e => e.PropertyProductId, "FKnj5gmnfl8qi7d1e4kq1367y8e");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.ImgUrl)
                .HasMaxLength(255)
                .HasColumnName("img_url");
            entity.Property(e => e.PropertyProductId)
                .HasMaxLength(36)
                .HasColumnName("property_product_id");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
            entity.Property(e => e.Value)
                .HasMaxLength(255)
                .HasColumnName("value");

            entity.HasOne(d => d.PropertyProduct).WithMany(p => p.PropertyValues)
                .HasForeignKey(d => d.PropertyProductId)
                .HasConstraintName("FKnj5gmnfl8qi7d1e4kq1367y8e");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("role");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("shop");

            entity.HasIndex(e => e.Status, "FKe7t0gp2cnsqiej8bu591yag9a");

            entity.HasIndex(e => e.UserId, "FKj97brjwss3mlgdt7t213tkchl");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.FollowersCount).HasColumnName("followers_count");
            entity.Property(e => e.FollowingCount).HasColumnName("following_count");
            entity.Property(e => e.FullNewAddress)
                .HasMaxLength(255)
                .HasColumnName("full_new_address");
            entity.Property(e => e.FullOldAddress)
                .HasMaxLength(255)
                .HasColumnName("full_old_address");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(255)
                .HasColumnName("logo_url");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.NewProviceId).HasColumnName("new_provice_id");
            entity.Property(e => e.NewWardId).HasColumnName("new_ward_id");
            entity.Property(e => e.OldDistrictId).HasColumnName("old_district_id");
            entity.Property(e => e.OldProviceId).HasColumnName("old_provice_id");
            entity.Property(e => e.OldWardId).HasColumnName("old_ward_id");
            entity.Property(e => e.PrdCount).HasColumnName("prd_count");
            entity.Property(e => e.Status)
                .HasColumnName("status");
            entity.Property(e => e.StreetLine)
                .HasMaxLength(255)
                .HasColumnName("street_line");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");

            // Removed StatusShopNavigation relationship as it's not defined in Shop entity

            entity.HasOne(d => d.User).WithMany(p => p.Shops)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FKj97brjwss3mlgdt7t213tkchl");
        });

        modelBuilder.Entity<TemplateRegisterCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("template_register_category");

            entity.HasIndex(e => e.CategoryId, "FKnmndc7f5i6kvin6y9gwmg5io6");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CategoryId)
                .HasMaxLength(36)
                .HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");

            entity.HasOne(d => d.Category).WithMany(p => p.TemplateRegisterCategories)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FKnmndc7f5i6kvin6y9gwmg5io6");
        });

        modelBuilder.Entity<TemplateRegisterCategoryChild>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("template_register_category_child");

            entity.HasIndex(e => e.CategoryChildId, "FK9s20wftp8cbkcprr6yqr8aw76");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CategoryChildId)
                .HasMaxLength(36)
                .HasColumnName("category_child_id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");

            entity.HasOne(d => d.CategoryChild).WithMany(p => p.TemplateRegisterCategoryChildren)
                .HasForeignKey(d => d.CategoryChildId)
                .HasConstraintName("FK9s20wftp8cbkcprr6yqr8aw76");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user");


            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.Email)
                .HasMaxLength(60)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasColumnName("gender");
            entity.Property(e => e.Name)
                .HasMaxLength(60)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
            entity.Property(e => e.HasShop)
                .HasColumnName("has_shop");
            entity.Property(e => e.Status)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");


            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserHasRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FKc1m07gjgx777ukpfw6wa94dfh"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FKdtkvc2iy3ph1rkvd67yl2t13m"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("user_has_role");
                        j.HasIndex(new[] { "RoleId" }, "FKc1m07gjgx777ukpfw6wa94dfh");
                        j.IndexerProperty<string>("UserId")
                            .HasMaxLength(36)
                            .HasColumnName("user_id");
                        j.IndexerProperty<string>("RoleId")
                            .HasMaxLength(36)
                            .HasColumnName("role_id");
                    });

            entity.HasMany(d => d.VoucherShops).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserVoucherShop",
                    r => r.HasOne<VoucherShop>().WithMany()
                        .HasForeignKey("VoucherShopId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FKk79iiyydhlw73h5kyveyqly1b"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK46t2uhh2ohfykbvwnxv9200ji"),
                    j =>
                    {
                        j.HasKey("UserId", "VoucherShopId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("user_voucher_shop");
                        j.HasIndex(new[] { "VoucherShopId" }, "FKk79iiyydhlw73h5kyveyqly1b");
                        j.IndexerProperty<string>("UserId")
                            .HasMaxLength(36)
                            .HasColumnName("user_id");
                        j.IndexerProperty<string>("VoucherShopId")
                            .HasMaxLength(36)
                            .HasColumnName("voucher_shop_id");
                    });

            entity.HasMany(d => d.VoucherSystems).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserVoucherSystem",
                    r => r.HasOne<VoucherSystem>().WithMany()
                        .HasForeignKey("VoucherSystemId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FKl23vdenx1uyhh7yhht4vkio70"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FKba40vpdxp40bla690bwpp7rmc"),
                    j =>
                    {
                        j.HasKey("UserId", "VoucherSystemId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("user_voucher_system");
                        j.HasIndex(new[] { "VoucherSystemId" }, "FKl23vdenx1uyhh7yhht4vkio70");
                        j.IndexerProperty<string>("UserId")
                            .HasMaxLength(36)
                            .HasColumnName("user_id");
                        j.IndexerProperty<string>("VoucherSystemId")
                            .HasMaxLength(36)
                            .HasColumnName("voucher_system_id");
                    });
        });

        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user_address");

            entity.HasIndex(e => e.UserId, "FKk2ox3w9jm7yd6v1m5f68xibry");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.FullNewAddress)
                .HasMaxLength(255)
                .HasColumnName("full_new_address");
            entity.Property(e => e.FullOldAddress)
                .HasMaxLength(255)
                .HasColumnName("full_old_address");
            entity.Property(e => e.NewProviceId).HasColumnName("new_provice_id");
            entity.Property(e => e.NewWardId).HasColumnName("new_ward_id");
            entity.Property(e => e.OldDistrictId).HasColumnName("old_district_id");
            entity.Property(e => e.OldProviceId).HasColumnName("old_provice_id");
            entity.Property(e => e.OldWardId).HasColumnName("old_ward_id");
            entity.Property(e => e.StreetLine)
                .HasMaxLength(255)
                .HasColumnName("street_line");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserAddresses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FKk2ox3w9jm7yd6v1m5f68xibry");
        });

        modelBuilder.Entity<Variant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("variant");

            entity.HasIndex(e => e.ProductId, "FKjjpllnln6hk6hj98uesgxno00");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductId)
                .HasMaxLength(36)
                .HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");

            entity.HasOne(d => d.Product).WithMany(p => p.Variants)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FKjjpllnln6hk6hj98uesgxno00");
        });

        modelBuilder.Entity<VariantValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("variant_value");

            entity.HasIndex(e => e.PropertyProductId, "FKf01sib30le97rkwg1rbvg9kx3");

            entity.HasIndex(e => e.PropertyValueId, "FKfsbgx2nyg0yp2lgtf29vronqf");

            entity.HasIndex(e => e.VariantId, "FKrx3kpmrdbxtml5wjxvmwuf1gd");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.PropertyProductId)
                .HasMaxLength(36)
                .HasColumnName("property_product_id");
            entity.Property(e => e.PropertyValueId).HasColumnName("property_value_id");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
            entity.Property(e => e.VariantId).HasColumnName("variant_id");

            entity.HasOne(d => d.PropertyProduct).WithMany(p => p.VariantValues)
                .HasForeignKey(d => d.PropertyProductId)
                .HasConstraintName("FKf01sib30le97rkwg1rbvg9kx3");

            entity.HasOne(d => d.PropertyValue).WithMany(p => p.VariantValues)
                .HasForeignKey(d => d.PropertyValueId)
                .HasConstraintName("FKfsbgx2nyg0yp2lgtf29vronqf");

            entity.HasOne(d => d.Variant).WithMany(p => p.VariantValues)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FKrx3kpmrdbxtml5wjxvmwuf1gd");
        });

        modelBuilder.Entity<VoucherShop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("voucher_shop");

            entity.HasIndex(e => e.ShopId, "FKs37q85rkionwjaqjl1of4xqro");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(55)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.DiscountValue)
                .HasPrecision(18, 3)
                .HasColumnName("discount_value");
            entity.Property(e => e.EndTime)
                .HasMaxLength(6)
                .HasColumnName("end_time");
            entity.Property(e => e.IsActive)
                .HasColumnType("bit(1)")
                .HasColumnName("is_active");
            entity.Property(e => e.MaxdiscountAmount)
                .HasPrecision(18, 3)
                .HasColumnName("maxdiscount_amount");
            entity.Property(e => e.MinimumOrderValue)
                .HasPrecision(18, 3)
                .HasColumnName("minimum_order_value");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ShopId)
                .HasMaxLength(36)
                .HasColumnName("shop_id");
            entity.Property(e => e.StartTime)
                .HasMaxLength(6)
                .HasColumnName("start_time");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");

            entity.HasOne(d => d.Shop).WithMany(p => p.VoucherShops)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("FKs37q85rkionwjaqjl1of4xqro");
        });

        modelBuilder.Entity<VoucherSystem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("voucher_system");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(55)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.DiscountValue)
                .HasPrecision(18, 3)
                .HasColumnName("discount_value");
            entity.Property(e => e.EndTime)
                .HasMaxLength(6)
                .HasColumnName("end_time");
            entity.Property(e => e.IsActive)
                .HasColumnType("bit(1)")
                .HasColumnName("is_active");
            entity.Property(e => e.MaxdiscountAmount)
                .HasPrecision(18, 3)
                .HasColumnName("maxdiscount_amount");
            entity.Property(e => e.MiniumOrderValue)
                .HasPrecision(18, 3)
                .HasColumnName("minium_order_value");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.StartTime)
                .HasMaxLength(6)
                .HasColumnName("start_time");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("wallet");

            entity.HasIndex(e => e.ShopId, "FK3haasvu2qmffnkk2nqxgt7q6w");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.Balance)
                .HasPrecision(15, 2)
                .HasColumnName("balance");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("created_by");
            entity.Property(e => e.ShopId)
                .HasMaxLength(36)
                .HasColumnName("shop_id");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updated_by");

            entity.HasOne(d => d.Shop).WithMany(p => p.Wallets)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("FK3haasvu2qmffnkk2nqxgt7q6w");
        });

        // Enum Mappings
        ConfigureEnumMappings(modelBuilder);

        OnModelCreatingPartial(modelBuilder);
    }

    /// <summary>
    /// Cấu hình enum mappings
    /// </summary>
    private void ConfigureEnumMappings(ModelBuilder modelBuilder)
    {
        // User enum mappings
        modelBuilder.Entity<User>()
            .Property(e => e.HasShop)
            .HasConversion<int>();

        modelBuilder.Entity<User>()
            .Property(e => e.Status)
            .HasConversion<int>();
        
        modelBuilder.Entity<User>()
            .Property(e => e.Gender)
            .HasConversion<int>();

        // Shop enum mappings
        modelBuilder.Entity<Shop>()
            .Property(e => e.Status)
            .HasConversion<int>();

        // Order enum mappings
        modelBuilder.Entity<Order>()
            .Property(e => e.StatusOrder)
            .HasConversion<int>();

        modelBuilder.Entity<Order>()
            .Property(e => e.PaymentMethod)
            .HasConversion<int>();

        modelBuilder.Entity<Order>()
            .Property(e => e.StatusPayment)
            .HasConversion<int>();

        modelBuilder.Entity<Order>()
            .Property(e => e.DeliveryStatus)
            .HasConversion<int>();

        // Payout enum mapping
        modelBuilder.Entity<Payout>()
            .Property(e => e.Status)
            .HasConversion<int>();

        // Voucher enum mappings
        modelBuilder.Entity<VoucherShop>()
            .Property(e => e.Type)
            .HasConversion<int>();

        modelBuilder.Entity<VoucherSystem>()
            .Property(e => e.Type)
            .HasConversion<int>();

        // Product enum mapping
        modelBuilder.Entity<Product>()
            .Property(e => e.Status)
            .HasConversion<int>();
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
