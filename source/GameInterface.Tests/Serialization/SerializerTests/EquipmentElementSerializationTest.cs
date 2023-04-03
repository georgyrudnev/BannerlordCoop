﻿using GameInterface.Serialization.External;
using GameInterface.Serialization;
using TaleWorlds.Core;
using Xunit;
using TaleWorlds.ObjectSystem;
using System.Reflection;
using GameInterface.Tests.Bootstrap;
using Autofac;
using GameInterface.Tests.Bootstrap.Modules;

namespace GameInterface.Tests.Serialization.SerializerTests
{
    public class EquipmentElementSerializationTest
    {
        IContainer container;
        public EquipmentElementSerializationTest()
        {
            GameBootStrap.Initialize();

            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterModule<SerializationTestModule>();

            container = builder.Build();
        }

        [Fact]
        public void EquipmentElement_Serialize()
        {
            EquipmentElement equipmentElement = new EquipmentElement();

            var factory = container.Resolve<IBinaryPackageFactory>();
            EquipmentElementBinaryPackage package = new EquipmentElementBinaryPackage(equipmentElement, factory);

            package.Pack();

            byte[] bytes = BinaryFormatterSerializer.Serialize(package);

            Assert.NotEmpty(bytes);
        }

        static FieldInfo _damage = typeof(ItemModifier).GetField("_damage", BindingFlags.Instance | BindingFlags.NonPublic);
        static FieldInfo _armor = typeof(ItemModifier).GetField("_armor", BindingFlags.Instance | BindingFlags.NonPublic);
        [Fact]
        public void EquipmentElement_Full_Serialization()
        {
            ItemObject itemobj = MBObjectManager.Instance.CreateObject<ItemObject>();
            ItemObject itemobj2 = MBObjectManager.Instance.CreateObject<ItemObject>();
            ItemModifier ItemModifier = MBObjectManager.Instance.CreateObject<ItemModifier>();

            ItemModifier.ModifyDamage(10);
            ItemModifier.ModifyArmor(15);

            EquipmentElement equipmentElement = new EquipmentElement(itemobj,ItemModifier,itemobj2);
            var factory = container.Resolve<IBinaryPackageFactory>();
            EquipmentElementBinaryPackage package = new EquipmentElementBinaryPackage(equipmentElement, factory);

            package.Pack();

            byte[] bytes = BinaryFormatterSerializer.Serialize(package);

            Assert.NotEmpty(bytes);

            object obj = BinaryFormatterSerializer.Deserialize(bytes);

            Assert.IsType<EquipmentElementBinaryPackage>(obj);

            EquipmentElementBinaryPackage returnedPackage = (EquipmentElementBinaryPackage)obj;

            EquipmentElement newEquipmentElement = returnedPackage.Unpack<EquipmentElement>();
            
            Assert.Equal(_damage.GetValue(equipmentElement.ItemModifier),
                         _damage.GetValue(newEquipmentElement.ItemModifier));

            Assert.Equal(_armor.GetValue(equipmentElement.ItemModifier),
                         _armor.GetValue(newEquipmentElement.ItemModifier));

            Assert.Equal(equipmentElement.Item.StringId, newEquipmentElement.Item.StringId);
            Assert.Equal(equipmentElement.CosmeticItem.StringId, newEquipmentElement.CosmeticItem.StringId);
        }
    }
}
