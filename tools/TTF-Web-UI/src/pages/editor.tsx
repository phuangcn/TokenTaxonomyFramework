import React, {Component} from "react";
import { connect } from 'react-redux';
import {
  Form,
  Input,
  Tooltip,
  Icon,
  Cascader,
  Select,
  Row,
  Col,
  Checkbox,
  Button,
  AutoComplete,
} from 'antd';
import {render} from "react-dom";
import {Taxonomy} from "../model/taxonomy_pb";
import {FormProps} from "antd/lib/form";
import KeyValue from "../components/form/keyvalue";
import {WrappedFormUtils} from "antd/lib/form/Form";
import {FormComponentProps} from "antd/es/form";
import {IEntity, ISidebarUI, IStoreState} from "../store/IStoreState";
import {Base} from "../model/core_pb";
import {Artifact} from "../model/artifact_pb";

const editable: boolean = true;
const formItemLayout = {
  labelCol: {
    xs: { span: 3 },
    sm: { span: 8 },
  },
  wrapperCol: {
    xs: { span: 24 },
    sm: { span: 16 },
  },
};

interface BaseFormProps extends FormComponentProps {
  selected: Artifact,
}

class BaseForm extends React.Component<BaseFormProps, any> {
  constructor(props: BaseFormProps) {
    super(props);
  }

  public render() {
    const {getFieldDecorator} = this.props.form;
    const form: WrappedFormUtils = this.props.form;
    const formName = this.props.selected.getName();

    return <div className={"form"}>
      <Form {...formItemLayout}>
        <h2>{formName}</h2>
        <div className="form-wrapper">
          <h3 dangerouslySetInnerHTML={{__html:"Edit " +
              // @ts-ignore
              this.props.selected.getArtifactSymbol().getVisual()}}/>
          <div className="inputs-wrapper">
            <Form.Item label="Name">
              {getFieldDecorator('name', {
                rules: [
                  {
                    required: true,
                    message: 'Please input a token name',
                  },
                ],
              })(<Input disabled={!editable}/>)}
            </Form.Item>
            <Form.Item label="Symbol">
              {getFieldDecorator('symbol', {
                rules: [
                  {
                    required: true,
                    message: 'Please input a token symbol',
                  },
                ],
              })(<Input disabled={!editable}/>)}
            </Form.Item>
            <Form.Item label="Owner">
              {getFieldDecorator('owner', {
                rules: [
                  {
                    required: true,
                    message: 'Please input a token owner',
                  },
                ],
              })(<Input disabled={!editable}/>)}
            </Form.Item>
            <Form.Item label="Quantity">
              {getFieldDecorator('quantity', {
                rules: [
                  {
                    required: true,
                    message: 'Please enter a quantity',
                  },
                ],
              })(<Input disabled={!editable}/>)}
            </Form.Item>
            <Form.Item label="Decimals">
              {getFieldDecorator('decimals', {
                rules: [
                  {
                    required: true,
                    message: 'Please enter the number of decimals allowed',
                  },
                ],
              })(<Input disabled={!editable}/>)}
            </Form.Item>
            <Form.Item label="Constructor name">
              {getFieldDecorator('constructor_name', {
                rules: [
                  {
                    required: true,
                    message: 'Please enter the name of the constructor',
                  },
                ],
              })(<Input disabled={!editable}/>)}
            </Form.Item>
          </div>
          <div className="add-wrapper">
            <Form.Item
              wrapperCol={{
                xs: {span: 24, offset: 8},
                sm: {span: 20, offset: 5},
              }}
            >
              {new KeyValue({field: "token_properties", form: form, label: "Token properties", disabled: !editable}).render()}
            </Form.Item>
          </div>
          {editable ?
            <div className="submit-wrapper">
              <Form.Item
                wrapperCol={{
                  xs: {span: 24, offset: 0},
                  sm: {span: 16, offset: 8},
                }}
              >
                <Button type="primary" htmlType="submit" className="submit">Submit</Button>
              </Form.Item>
            </div>: null}
        </div>
      </Form>
    </div>
  }
};

const Editor = Form.create<BaseFormProps>({
  mapPropsToFields: (props: BaseFormProps) => {
    // @ts-ignore
    const symbol = props.selected.getArtifactSymbol().getVisual();
    return {name: Form.createFormField({
        name: props.selected.getName(),
        value: props.selected.getName(),
      }),
      symbol: Form.createFormField({
        name: symbol,
        value: symbol,
      })};
  }
})(BaseForm);

export default connect(
  (state: IStoreState) => {
    // @ts-ignore
    const currentLocation = window.location.href.substring(window.location.href.lastIndexOf('/') + 1);
    // @ts-ignore
    let selected = state.entities.bases.byId.get(currentLocation);
    if (selected == undefined) {
      // @ts-ignore
      selected = state.entities.behaviors.byId.get(currentLocation);
    }
    if (selected == undefined) {
      // @ts-ignore
      selected = state.entities.behaviorGroups.byId.get(currentLocation);
    }
    if (selected == undefined) {
      // @ts-ignore
      selected = state.entities.propertySets.byId.get(currentLocation);
    }
    if (selected == undefined) {
      // @ts-ignore
      selected = state.entities.templateDefinitions.byId.get(currentLocation);
    }

    if (selected == undefined) {
      throw "should not be there";
    }
    return {
      selected: selected
    }
  },
  { },
  null,
  {
    pure: false
  }
)(Editor);